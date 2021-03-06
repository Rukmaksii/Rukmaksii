using System;
using GameScene.Abilities.model;

namespace GameScene.PlayerControllers
{
    public class SoldierClassPlayer : BasePlayer.BasePlayer
    {
        public override string ClassName { get; } = "Soldier";
        public override int MaxHealth { get; set; } = 100;
        public override RootAbility RootAbility { get; } = new SoldierClassRoot();
        protected override float movementSpeed { get; } = 7.5f;
        public override float JetpackForce { get; set; } = 20f;

        protected override float runningSpeedMultiplier { get; } = 1.75f;

        protected override float jumpForce { get; } = 5;

        public override float DefaultFuelDuration { get; } = 10;

        protected override float dashDuration { get; } = .3f;

        protected override float dashForce { get; } = 30f;

        protected override float gravityMultiplier { get; } = 1f;

        public override Type WeaponInterface { get; } = typeof(IWeapon);

        public interface IWeapon : model.IWeapon
        {
        }
    }
}