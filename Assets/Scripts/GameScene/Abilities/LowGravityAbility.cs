using System;
using System.Collections.Generic;
using GameScene.PlayerControllers.BasePlayer;

namespace GameScene.Abilities
{
    public class LowGravityAbility : BaseAbility
    {
        private float multiplier = 0.7f;

        public override List<Type> Children { get; } = new List<Type>{typeof(JumpAbility), typeof(SpeedAbility)};

        
        public override void Apply()
        {
            Player.EffectiveGravityMultiplier *= multiplier;
        }

        public LowGravityAbility(BasePlayer player) : base(player)
        {
        }
    }
}