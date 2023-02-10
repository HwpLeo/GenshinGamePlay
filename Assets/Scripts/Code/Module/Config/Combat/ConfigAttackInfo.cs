﻿using System.Collections.Generic;
using UnityEngine;

namespace TaoTie
{
    public class ConfigAttackInfo
    {
        [Tooltip("攻击标识，其他地方可通过这个标记筛选过滤")]
        public string AttackTag;
        [Tooltip("衰减类型标记")]
        public string AttenuationTag;
        [Tooltip("衰减分组")]
        public string AttenuationGroup;
        [Tooltip("伤害数据")] [NotNull]
        public ConfigAttackProperty AttackProperty;
        [Tooltip("打击数据")]
        public Dictionary<HitBoxType, ConfigHitPattern> HitPattern;
        [Tooltip("强制抖动摄像机")]
        public bool ForceCameraShake;
        [Tooltip("抖动摄像机参数")]
        public ConfigCameraShake CameraShake;
        [Tooltip("子弹衰减")]
        public ConfigBulletWane BulletWane;
    }
}