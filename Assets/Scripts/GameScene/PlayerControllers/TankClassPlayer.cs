﻿using System;
using GameScene.Abilities.model;

namespace GameScene.PlayerControllers
{
    public class TankClassPlayer : BasePlayer.BasePlayer
    {
        public override string ClassName { get; } = "Tank";
        public override int MaxHealth { get; set; } = 125;
        public override RootAbility RootAbility { get; } = new TankClassRoot();
        protected override float movementSpeed { get; } = 5f;

        protected override float runningSpeedMultiplier { get; } = 1.75f;

        protected override float jumpForce { get; } = 3;

        public override float DefaultFuelDuration { get; } = 7.5f;

        public override float dashDuration { get; set; } = .2f;

        protected override float dashForce { get; } = 22.5f;

        public override float gravityMultiplier { get; set; } = 0.6f;

        public override Type WeaponInterface { get; } = typeof(IWeapon);

        public interface IWeapon : model.IWeapon
        {
        }
    }
}