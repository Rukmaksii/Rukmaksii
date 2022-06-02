using System;
using GameScene.Abilities.model;

namespace GameScene.PlayerControllers
{
    public class ScoutClassPlayer : BasePlayer.BasePlayer
    {
        public override string ClassName { get; } = "Scout";
        public override int MaxHealth { get; protected set; } = 75;
        public override RootAbility RootAbility { get; } = new ScoutClassRoot();
        protected override float movementSpeed { get; } = 7.5f;

        protected override float runningSpeedMultiplier { get; } = 2.25f;

        protected override float jumpForce { get; } = 7.5f;

        public override float DefaultFuelDuration { get; } = 12.5f;

        protected override float dashDuration { get; } = .2f;

        protected override float dashForce { get; } = 45f;

        protected override float gravityMultiplier { get; } = 1.3f;

        public override Type WeaponInterface { get; } = typeof(IWeapon);

        public interface IWeapon : model.IWeapon
        {
        }
    }
}