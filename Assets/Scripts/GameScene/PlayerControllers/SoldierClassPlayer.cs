﻿using System;
using GameScene.Abilities.model;

namespace GameScene.PlayerControllers
{
    public class SoldierClassPlayer : BasePlayer.BasePlayer
    {
        public override string ClassName { get; } = "Soldier";
        public override int MaxHealth { get; set; } = 100;
        public override RootAbility RootAbility { get; } = new SoldierClassRoot();
        protected override float movementSpeed { get; } = 7.5f;

        protected override float runningSpeedMultiplier { get; } = 1.75f;

        protected override float jumpForce { get; } = 5;

        public override float DefaultFuelDuration { get; } = 10;

        public override float dashDuration { get; set; } = .3f;

        protected override float dashForce { get; } = 30f;

        public override float gravityMultiplier { get; set; } = 1f;

        public override Type WeaponInterface { get; } = typeof(IWeapon);

        public interface IWeapon : model.IWeapon
        {
        }
    }
}