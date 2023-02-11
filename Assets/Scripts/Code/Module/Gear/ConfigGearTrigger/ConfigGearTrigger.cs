﻿using System;

using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;

namespace TaoTie
{
    // Trigger
    [Serializable]
    public abstract class ConfigGearTrigger
    {
        [PropertyOrder(int.MinValue)] [SerializeField]
        public int localId;

        [SerializeReference][OnCollectionChanged("Refresh")] [OnStateUpdate("Refresh")] 
        public ConfigGearAction[] actions;

#if UNITY_EDITOR
        [PropertyOrder(int.MinValue + 1)] [LabelText("策划备注")] [SerializeField]
        private string remarks;
        
        private void Refresh()
        {
            if (actions == null) return;
            for (int i = 0; i < actions.Length; i++)
            {
                if (actions[i] != null)
                    actions[i].handleType = GetType();
            }

            actions.Sort((a, b) => { return a.localId - b.localId; });
        }
#endif
    }
}