using System;
using System.Collections.Generic;
using GameScene.PlayerControllers.BasePlayer;

namespace GameScene.Abilities
{
    public class SuperSpeedAbility : BaseAbility
    {
        private float multiplier = 2.5f;

        public override List<Type> Children { get; } = new List<Type>{typeof(JumpAbility)};

        
        public override void Apply()
        {
            Player.EffectiveMovementSpeed *= multiplier;
            Player.JetpackForce *= multiplier;
        }

        public SuperSpeedAbility(BasePlayer player) : base(player)
        {
        }
    }
}