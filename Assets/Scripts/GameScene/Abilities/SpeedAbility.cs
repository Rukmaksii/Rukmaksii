using System;
using System.Collections.Generic;
using GameScene.PlayerControllers.BasePlayer;

namespace GameScene.Abilities
{
    public class SpeedAbility : BaseAbility
    {
        private float multiplier = 1.5f;

        public override List<Type> Children { get; } = new List<Type>{typeof(JumpAbility)};

        
        public override void Apply()
        {
            Player.movementSpeed *= multiplier;
            Player.Jetpack.JetpackForce *= multiplier;
        }

        public SpeedAbility(BasePlayer player) : base(player)
        {
        }
    }
}