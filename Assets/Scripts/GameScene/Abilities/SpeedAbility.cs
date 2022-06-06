using System;
using System.Collections.Generic;
using GameScene.PlayerControllers.BasePlayer;

namespace GameScene.Abilities
{
    public class SpeedAbility : BaseAbility
    {
        private float multiplier = 1.5f;

        public override List<Type> Children { get; } = new List<Type>{typeof(JumpAbility), typeof(SuperSpeedAbility)};

        
        public override void Apply()
        {
            Player.EffectiveMovementSpeed *= multiplier;
            Player.JetpackForce *= multiplier;
        }

        public SpeedAbility(BasePlayer player) : base(player)
        {
        }
    }
}