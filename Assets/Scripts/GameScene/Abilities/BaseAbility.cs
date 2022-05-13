using System;
using System.Collections.Generic;
using GameScene.model;
using GameScene.PlayerControllers;
using GameScene.PlayerControllers.BasePlayer;

namespace GameScene.Abilities
{
    public struct AbilityInfo
    {
        public string Name;
    }

    public abstract class BaseAbility : IAbility
    {
        public readonly static Dictionary<Type, AbilityInfo> AbilityInfos = new Dictionary<Type, AbilityInfo>()
        {
            {
                typeof(Jetpack), new AbilityInfo()
                {
                    Name = "Jetpack Ability",
                }
            }
        };

        protected BasePlayer Player;
        public AbilityInfo Info => AbilityInfos[GetType()];

        public abstract List<Type> Children { get; }


        public BaseAbility(BasePlayer player)
        {
            Player = player;
        }

        public abstract void Apply();
    }
}