﻿using System;
using GameScene.Abilities.model;

namespace GameScene.PlayerControllers
{
    public class SoldierClassPlayer : BasePlayer.BasePlayer
    {
        public override string ClassName { get; } = "test class";
        public override int MaxHealth { get; protected set; } = 100;
        public override RootAbility RootAbility { get; } = new TestClassRoot();
        protected override float movementSpeed { get; } = 10f;

        public override Type WeaponInterface { get; } = typeof(SoldierClassPlayer.IWeapon);

        public interface IWeapon : model.IWeapon
        {
        }
    }
}