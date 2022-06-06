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
            },
            {
                typeof(DashAbility), new AbilityInfo()
                {
                    Name = "Dash Ability",
                    Description = "+0.5s of dash duration",
                    Price = 200,
                    SpritePath = "Abilities/Dash"
                }
            },
            {
                typeof(JumpAbility), new AbilityInfo()
                {
                    Name = "Jump Ability",
                    Description = "+100% of jump force",
                    Price = 400,
                    SpritePath = "Abilities/Jump"
                }
            },
            {
                typeof(SpeedAbility), new AbilityInfo()
                {
                    Name = "Speed Ability",
                    Description = "+50% of movement speed",
                    Price = 300,
                    SpritePath = "Abilities/Speed"
                }
            },
            {
                typeof(PickupRangeAbility), new AbilityInfo()
                {
                    Name = "Pickup Range Ability",
                    Description = "+100% of pickup range",
                    Price = 200,
                    SpritePath = "Abilities/Luffy"
                }
            },
            {
                typeof(LowGravityAbility), new AbilityInfo()
                {
                    Name = "LowGravity Ability",
                    Description = "-30% of Gravity",
                    Price = 200,
                    SpritePath = "Abilities/Newton"
                }
            },
            {
                typeof(SuperSpeedAbility), new AbilityInfo()
                {
                    Name = "Super Speed Ability",
                    Description = "+300% of movement speed",
                    Price = 300,
                    SpritePath = "Abilities/Sanic"
                }
            },
            {
                typeof(DamageMultiplierAbility), new AbilityInfo()
                {
                    Name = "Damage Multiplier Ability",
                    Description = "+30% of damage done",
                    Price = 500,
                    SpritePath = "Abilities/Stronk"
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