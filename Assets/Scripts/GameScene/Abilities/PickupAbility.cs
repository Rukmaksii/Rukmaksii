using System;
using System.Collections.Generic;
using GameScene.PlayerControllers.BasePlayer;

namespace GameScene.Abilities
{
    public class PickupRangeAbility : BaseAbility
    {
        private float multiplier = 2f;

        public override List<Type> Children { get; } = new List<Type> {typeof(SpeedAbility), typeof(DamageMultiplierAbility), typeof(JetpackAbility)};

        
        public override void Apply()
        {
            Player.PickUpDistance *= multiplier;
        }

        public PickupRangeAbility(BasePlayer player) : base(player)
        {
        }
    }
}