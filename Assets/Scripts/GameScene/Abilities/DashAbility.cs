using System;
using System.Collections.Generic;
using GameScene.PlayerControllers.BasePlayer;

namespace GameScene.Abilities
{
    public class DashAbility : BaseAbility
    {
        private float multiplier = 0.5f;

        public override List<Type> Children { get; } = new List<Type>{typeof(DashAbility), typeof(JumpAbility), typeof(SpeedAbility)};

        public override void Apply()
        {
            Player.EffectiveDashDuration *= multiplier;
        }

        public DashAbility(BasePlayer player) : base(player)
        {
        }
    }
}