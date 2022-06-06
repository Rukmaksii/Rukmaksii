using System;
using System.Collections.Generic;
using GameScene.PlayerControllers.BasePlayer;

namespace GameScene.Abilities
{
    public class DamageMultiplierAbility : BaseAbility
    {
        private float multiplier = 1.3f;

        public override List<Type> Children { get; } = new List<Type> {typeof(DamageMultiplierAbility), typeof(DashAbility), typeof(PickupRangeAbility)};

        
        public override void Apply()
        {
            Player.damageMultiplier *= multiplier;
        }

        public DamageMultiplierAbility(BasePlayer player) : base(player)
        {
        }
    }
}