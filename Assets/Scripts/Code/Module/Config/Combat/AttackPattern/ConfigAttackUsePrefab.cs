﻿using Nino.Serialization;

namespace TaoTie
{
    [NinoSerialize]
    public partial class ConfigAttackUsePrefab: ConfigBaseAttackPattern
    {
        [NinoMember(10)]
        public string PrefabPathName;

        public override int ResolveHit(Entity applier, ActorAbility ability, ActorModifier modifier, Entity target, EntityType[] filter,
            out HitInfo[] hitInfos)
        {
            throw new System.NotImplementedException();
        }
    }
}