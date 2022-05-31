using System;
using System.Collections.Generic;
using GameScene.model;
using GameScene.PlayerControllers;
using GameScene.PlayerControllers.BasePlayer;

namespace GameScene.Abilities
{
    public struct AbilityInfo
    {
        /**
         * the name of the ability
         */
        public string Name;
        
        /**
         * the description of the stats changes applied by the ability
         */
        public string Description;
        
        /**
         * the cost of the ability
         */
        public int Price;

        /**
         * the path to the sprite in resources/Abilities/
         */
        public string SpritePath;
    }

    public abstract class BaseAbility : IAbility
    {
        public readonly static Dictionary<Type, AbilityInfo> AbilityInfos = new Dictionary<Type, AbilityInfo>()
        {
            {
                typeof(Jetpack), new AbilityInfo()
                {
                    Name = "Jetpack Ability",
                    Description = "+10% for jetpack",
                    Price = 500,
                    SpritePath = "Abilities/Jetpack"
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