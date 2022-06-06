using System;
using System.Collections.Generic;
using GameScene.PlayerControllers.BasePlayer;

namespace GameScene.Abilities
{
    public class HealthAbility : BaseAbility
    {
        private int multiplier = 20;

        public override List<Type> Children { get; } = new List<Type> {typeof(HealthAbility), typeof(JumpAbility), typeof(PickupRangeAbility)};

        public override void Apply()
        {
            Player.MaxHealth += multiplier;
            Player.CurrentHealth += multiplier;
        }

        public HealthAbility(BasePlayer player) : base(player)
        {
        }
    }
}