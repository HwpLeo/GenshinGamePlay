﻿using UnityEngine;

namespace TaoTie
{
    public class BillboardNamePlugin: BillboardPlugin<ConfigBillboardNamePlugin>
    {
        private TextMesh font;
        private GameObject obj;
        #region BillboardPlugin

        protected override void InitInternal()
        {
            InitInternalAsync().Coroutine();
        }
        private async ETTask InitInternalAsync()
        {
            var goh = billboardComponent.GetParent<Entity>().GetComponent<GameObjectHolderComponent>();
            await goh.WaitLoadGameObjectOver();
            if(goh.IsDispose || billboardComponent.IsDispose) return;
            
            var pointer = goh.GetCollectorObj<Transform>(billboardComponent.Config.AttachPoint);
            if (pointer == null)
            {
                GameObjectPoolManager.Instance.RecycleGameObject(obj);
                return;
            }
            
            obj = new GameObject("Name");
            obj.transform.localScale = 0.1f * Vector3.one;
            obj.transform.SetParent(pointer);
            obj.transform.localPosition = billboardComponent.Config.Offset + config.Offset;
            var mainC = CameraManager.Instance.MainCamera();
            if (mainC != null && obj != null)
            {
                obj.transform.rotation = mainC.transform.rotation;
                obj.transform.localPosition = obj.transform.localRotation*(billboardComponent.Config.Offset + config.Offset);
            }
            font = obj.AddComponent<TextMesh>();
            font.fontSize = 36;
            font.alignment = TextAlignment.Center;
            font.anchor = TextAnchor.MiddleCenter;
            font.color = Color.black;
            font.text = "Test";
        }

        protected override void UpdateInternal()
        {
            var mainC = CameraManager.Instance.MainCamera();
            if (mainC != null && obj != null)
            {
                obj.transform.rotation = mainC.transform.rotation;
                obj.transform.localPosition = obj.transform.localRotation*(billboardComponent.Config.Offset + config.Offset);
            }
        }

        protected override void DisposeInternal()
        {
            font = null;
            if (obj != null)
            {
                Object.Destroy(obj);
                obj = null;
            }
        }
        
        #endregion
    }
}