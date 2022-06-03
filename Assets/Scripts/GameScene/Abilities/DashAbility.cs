﻿using System;
using System.Collections.Generic;
using GameScene.PlayerControllers.BasePlayer;

namespace GameScene.Abilities
{
    public class DashAbility : BaseAbility
    {
        private float multiplier = 0.5f;

        public override List<Type> Children { get; } = new List<Type>();

        public override void Apply()
        {
            Player.dashDuration *= multiplier;
        }

        public DashAbility(BasePlayer player) : base(player)
        {
        }
    }
}