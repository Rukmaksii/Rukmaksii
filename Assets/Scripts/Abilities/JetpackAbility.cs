using System;
using System.Collections.Generic;
using PlayerControllers;
using UnityEngine;

namespace Abilities
{
    public class JetpackAbility : BaseAbility
    {
        private float multiplier = 1.1f;

        public override List<BaseAbility> Children { get; } = new List<BaseAbility>();

        public override void Apply()
        {
            Player.Jetpack.FuelDuration *= multiplier;
        }

        public JetpackAbility(BasePlayer player) : base(player)
        {
        }
    }
}