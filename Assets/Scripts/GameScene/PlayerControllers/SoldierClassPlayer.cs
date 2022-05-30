using System;
using GameScene.Abilities.model;

namespace GameScene.PlayerControllers
{
    public class SoldierClassPlayer : BasePlayer.BasePlayer
    {
        public override string ClassName { get; } = "test class";
        public override int MaxHealth { get; protected set; } = 100;
        public override RootAbility RootAbility { get; } = new TestClassRoot();
        protected override float movementSpeed { get; } = 7.5f;

        protected override float runningSpeedMultiplier { get; } = 1.75f;

        protected override float jumpForce { get; } = 5;

        public override float DefaultFuelDuration { get; } = 10;

        protected override float dashDuration { get; set; } = .3f;

        protected override float dashForce { get; set; } = 30f;

        protected override float gravityMultiplier { get; } = 1f;

        public override Type WeaponInterface { get; } = typeof(SoldierClassPlayer.IWeapon);

        public interface IWeapon : model.IWeapon
        {
        }
    }
}