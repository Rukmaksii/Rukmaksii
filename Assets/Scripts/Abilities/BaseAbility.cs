using System;
using System.Collections.Generic;
using model;
using PlayerControllers;

namespace Abilities
{
    public struct AbilityInfo
    {
        public string Name;
        public List<Type> ChildrenAbilities;
    }

    public abstract class BaseAbility : IAbility
    {
        public readonly static Dictionary<Type, AbilityInfo> AbilityInfos = new Dictionary<Type, AbilityInfo>()
        {
            {
                typeof(Jetpack), new AbilityInfo()
                {
                    Name = "Jetpack Ability",
                    ChildrenAbilities = new List<Type>
                    {
                    }
                }
            }
        };

        protected BasePlayer Player;
        public AbilityInfo Info => AbilityInfos[GetType()];


        public BaseAbility(BasePlayer player)
        {
            Player = player;
        }

        public abstract void Apply();
    }
}