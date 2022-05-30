using GameScene.model;
using GameScene.PlayerControllers;

namespace GameScene.Weapons.HeavyWeapons
{
    public class M4 : BaseWeapon, SoldierClassPlayer.IWeapon, TankClassPlayer.IWeapon
    {
        public override WeaponType Type { get; } = WeaponType.Heavy;
        public override float Range { get; } = 100f;
        public override int Damage { get; } = 4;
        public override float Cooldown { get; } = 0.1f;
        public override int MaxAmmo { get; } = 30;
        public override float ReloadTime { get; } = 1.5f;
        public override int BulletsInRow { get; } = 1;
        public override float BulletsInRowSpacing { get; } = 0.1f;
        public override int Price { get; } = 900;
        public override string Name { get; } = "M4";
    }
}