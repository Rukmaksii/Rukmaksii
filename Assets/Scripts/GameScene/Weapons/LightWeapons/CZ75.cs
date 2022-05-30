using GameScene.model;
using GameScene.PlayerControllers;

namespace GameScene.Weapons
{
    public class CZ75 : BaseWeapon, TankClassPlayer.IWeapon
    {
        public override WeaponType Type { get; } = WeaponType.Light;
        public override float Range { get; } = 25f;
        public override int Damage { get; } = 2;
        public override float Cooldown { get; } = 0.08f;
        public override int MaxAmmo { get; } = 15;
        public override float ReloadTime { get; } = 2f;
        public override int BulletsInRow { get; } = 1;
        public override float BulletsInRowSpacing { get; } = 0.1f;
        public override string Name { get; } = "CZ75";
    }
}