using System;
using System.Collections.Generic;
using PlayerControllers;
using UnityEngine;

namespace Abilities
{
    public class JetpackAbility : BaseAbility
    {
        public override string Name { get; } = "jetpack";
        public override List<Type> ChildrenAbilities { get; } = new List<Type>();
        private float multiplier = 1.1f;

        public override void Apply()
        {
            Player.Jetpack.FuelDuration *= multiplier;
        }

        public JetpackAbility(BasePlayer player) : base(player)
        {
        }
    }
}