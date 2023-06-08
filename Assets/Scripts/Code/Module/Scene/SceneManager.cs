﻿using System;
using System.Collections.Generic;
using UnityEngine;

namespace TaoTie
{
    /// <summary>
    /// 场景管理系统：调度和控制场景异步加载以及进度管理，展示loading界面和更新进度条数据，GC、卸载未使用资源等
    /// 注意：
    /// 资源预加载放各个场景类中自行控制
    /// </summary>
    public class SceneManager:IManager
    {
        public static SceneManager Instance;

        private Dictionary<Type, IScene> scenes;

        private Dictionary<string, Type> nameMapScenes;
        //当前场景
        public IScene CurrentScene;
        //是否忙
        public bool Busing = false;
        
        private readonly Queue<ETTask> waitFinishTask = new Queue<ETTask>();
        #region override

        public void Init()
        {
            scenes = new Dictionary<Type, IScene>();
            nameMapScenes = new Dictionary<string, Type>();
            Instance = this;
            var allTypes = AssemblyManager.Instance.GetTypes();
            var scene = TypeInfo<IScene>.Type;
            foreach (var item in allTypes)
            {
                var type = item.Value;
                if (!type.IsAbstract && scene.IsAssignableFrom(type))
                {
                    nameMapScenes.Add(type.Name, type);
                }
            }
        }

        public void Destroy()
        {
            nameMapScenes.Clear();
            waitFinishTask.Clear();
            nameMapScenes = null;
            scenes = null;
            Instance = null;
        }

        #endregion

        async ETTask<IScene> GetScene<T>() where T : IScene
        {
            if (scenes.TryGetValue(TypeInfo<T>.Type, out var res))
            {
                return res;
            }
            res = Activator.CreateInstance<T>();
            await res.OnCreate();
            scenes.Add(TypeInfo<T>.Type,res);
            return res;
        }
        
        async ETTask<IScene> GetScene(Type type)
        {
            if (scenes.TryGetValue(type, out var res))
            {
                return res;
            }
            res = Activator.CreateInstance(type) as IScene;
            await res.OnCreate();
            scenes.Add(type,res);
            return res;
        }

        //切换场景
        async ETTask InnerSwitchScene(Type type, bool needClean = false)
        {
            float slidValue = 0;
            Log.Info("InnerSwitchScene start open uiLoading");
            var scene = await GetScene(type);
            if(CurrentScene!=null)
                await CurrentScene.OnLeave();
            await scene.OnEnter();
            await scene.SetProgress(slidValue);

            CameraManager.Instance.SetCameraStackAtLoadingStart();

            //等待资源管理器加载任务结束，否则很多Unity版本在切场景时会有异常，甚至在真机上crash
            Log.Info("InnerSwitchScene ProcessRunning Done ");
            while (ResourcesManager.Instance.IsProcessRunning())
            {
                await TimerManager.Instance.WaitAsync(1);
            }
            slidValue += 0.01f;
            await scene.SetProgress(slidValue);
            await TimerManager.Instance.WaitAsync(1);

            //清理UI
            Log.Info("InnerSwitchScene Clean UI");
            await UIManager.Instance.DestroyWindowExceptNames(scene.GetDontDestroyWindow());
            
            slidValue += 0.01f;
            await scene.SetProgress(slidValue);
            //清除ImageLoaderManager里的资源缓存 这里考虑到我们是单场景
            Log.Info("InnerSwitchScene ImageLoaderManager Cleanup");
            ImageLoaderManager.Instance.Clear();
            //清除预设以及其创建出来的gameObject, 这里不能清除loading的资源
            Log.Info("InnerSwitchScene GameObjectPool Cleanup");
            if (needClean && CurrentScene != null)
            {
                GameObjectPoolManager.Instance.Cleanup(true, CurrentScene.GetScenesChangeIgnoreClean());
                slidValue += 0.01f;
                await scene.SetProgress(slidValue);
                //清除除loading外的资源缓存 
                List<UnityEngine.Object> gos = new List<UnityEngine.Object>();
                for (int i = 0; i < CurrentScene.GetScenesChangeIgnoreClean().Count; i++)
                {
                    var path = CurrentScene.GetScenesChangeIgnoreClean()[i];
                    var go = GameObjectPoolManager.Instance.GetCachedGoWithPath(path);
                    if (go != null)
                    {
                        gos.Add(go);
                    }
                }
                Log.Info("InnerSwitchScene ResourcesManager ClearAssetsCache excludeAssetLen = " + gos.Count);
                ResourcesManager.Instance.ClearAssetsCache(gos.ToArray());
                slidValue += 0.01f;
                await scene.SetProgress(slidValue);
            }
            else
            {
                slidValue += 0.02f;
                await scene.SetProgress(slidValue);
            }

            var loadingScene = await GetScene<LoadingScene>();
            await ResourcesManager.Instance.LoadSceneAsync(loadingScene.GetScenePath(), false);
            Log.Info("LoadSceneAsync Over");
            slidValue += 0.01f;
            await scene.SetProgress(slidValue);
            //GC：交替重复2次，清干净一点
            GC.Collect();
            GC.Collect();

            var res = Resources.UnloadUnusedAssets();
            while (!res.isDone)
            {
                await TimerManager.Instance.WaitAsync(1);
            }
            slidValue += 0.12f;
            await scene.SetProgress(slidValue);

            Log.Info("异步加载目标场景 Start");
            //异步加载目标场景
            await ResourcesManager.Instance.LoadSceneAsync(scene.GetScenePath(), false);
            await scene.OnComplete();
            slidValue += 0.65f;
            await scene.SetProgress(slidValue);
            //准备工作：预加载资源等
            await scene.OnPrepare();

            slidValue += 0.15f;
            await scene.SetProgress(slidValue);
            CameraManager.Instance.SetCameraStackAtLoadingDone();

            slidValue = 1;
            await scene.SetProgress(slidValue);
            Log.Info("等久点，跳的太快");
            //等久点，跳的太快
            await TimerManager.Instance.WaitAsync(500);
            Log.Info("加载目标场景完成 Start");
            CurrentScene = scene;
            await scene.OnSwitchSceneEnd();
            FinishLoad();
        }
        //切换场景
        public async ETTask SwitchScene<T>(bool needClean = false)where T:IScene
        {
            if (this.Busing) return;
            if (IsInTargetScene<T>())
                return;
            this.Busing = true;

            await this.InnerSwitchScene(TypeInfo<T>.Type, needClean);

            //释放loading界面引用的资源
            GameObjectPoolManager.Instance.CleanupWithPathArray(true, CurrentScene.GetScenesChangeIgnoreClean());
            this.Busing = false;
        }
        
        //切换场景
        public async ETTask SwitchMapScene(string typeName, bool needClean = false)
        {
            if (this.Busing) return;
            if(!nameMapScenes.TryGetValue(typeName,out var type)) return;
            if (IsInTargetScene(type))
                return;
            this.Busing = true;

            await this.InnerSwitchScene(type, needClean);

            //释放loading界面引用的资源
            GameObjectPoolManager.Instance.CleanupWithPathArray(true, CurrentScene.GetScenesChangeIgnoreClean());
            this.Busing = false;
        }

        public IScene GetCurrentScene()
        {
            return this.CurrentScene;
        }
        public T GetCurrentScene<T>() where T:IScene
        {
            return (T)this.CurrentScene;
        }
        public bool IsInTargetScene<T>()where T:IScene
        {
            if (this.CurrentScene == null) return false;
            return this.CurrentScene.GetType() == TypeInfo<T>.Type;
        }
        public bool IsInTargetScene(Type type)
        {
            if (this.CurrentScene == null) return false;
            return this.CurrentScene.GetType() == type;
        }
        public ETTask WaitLoadOver()
        {
            ETTask task = ETTask.Create();
            waitFinishTask.Enqueue(task);
            return task;
        }
        
        public void FinishLoad()
        {
            int count = waitFinishTask.Count;
            while (count-- > 0)
            {
                ETTask task = waitFinishTask.Dequeue();
                task.SetResult();
            }
        }
        
    }
}