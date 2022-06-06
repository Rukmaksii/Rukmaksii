using System;
using System.Collections.Generic;
using GameScene.PlayerControllers.BasePlayer;

namespace GameScene.Abilities
{
    public class JumpAbility : BaseAbility
    {
        private int multiplier = 2;

        public override List<Type> Children { get; } = new List<Type> {typeof(JumpAbility), typeof(LowGravityAbility), typeof(SpeedAbility)};

        public override void Apply()
        {
            Player.EffectiveJumpForce *= multiplier;
        }

        public JumpAbility(BasePlayer player) : base(player)
        {
        }
    }
}