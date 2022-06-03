using System;
using GameScene.Abilities.model;

namespace GameScene.PlayerControllers
{
    public class ScoutClassPlayer : BasePlayer.BasePlayer
    {
        public override string ClassName { get; } = "Scout";
        public override int MaxHealth { get; set; } = 75;
        public override RootAbility RootAbility { get; } = new ScoutClassRoot();
        public override float movementSpeed { get; set; } = 7.5f;

        protected override float runningSpeedMultiplier { get; } = 2.25f;

        public override float jumpForce { get; set; } = 7.5f;

        public override float DefaultFuelDuration { get; } = 12.5f;

        public override float dashDuration { get; set; } = .2f;

        protected override float dashForce { get; } = 45f;

        public override float gravityMultiplier { get; set; } = 1.3f;

        public override Type WeaponInterface { get; } = typeof(IWeapon);

        public interface IWeapon : model.IWeapon
        {
        }
    }
}