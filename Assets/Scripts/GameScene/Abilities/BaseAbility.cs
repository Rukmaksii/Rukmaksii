using System;
using System.Collections.Generic;
using GameScene.model;
using GameScene.PlayerControllers;
using GameScene.PlayerControllers.BasePlayer;
using UnityEngine;

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

        public Sprite Sprite => Resources.Load<Sprite>(SpritePath);
    }

    public abstract class BaseAbility : IAbility
    {
        public readonly static Dictionary<Type, AbilityInfo> AbilityInfos = new Dictionary<Type, AbilityInfo>()
        {
            {
                typeof(JetpackAbility), new AbilityInfo()
                {
                    Name = "Jetpack Ability",
                    Description = "+10% for jetpack",
                    Price = 500,
                    SpritePath = "Abilities/Jetpack"
                }
            },
            {
                typeof(HealthAbility), new AbilityInfo()
                {
                    Name = "Health Ability",
                    Description = "+20 Health points",
                    Price = 300,
                    SpritePath = "Abilities/Health"
                }
            }
        };

        protected BasePlayer Player;
        public AbilityInfo Info => AbilityInfos[GetType()];

        public Sprite Sprite => Info.Sprite;

        public string Description => Info.Description;
        public string Name => Info.Name;
        public int Price => Info.Price;

        public abstract List<Type> Children { get; }
        


        public BaseAbility(BasePlayer player)
        {
            if (player != null && !AbilityInfos.ContainsKey(GetType()))
                throw new Exception($"item {GetType().Name} is not referenced in BaseAbilitiy::AbilityInfos");
            Player = player;
        }

        public abstract void Apply();
    }
}