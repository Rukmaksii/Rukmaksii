using GameScene.model;
using GameScene.PlayerControllers;

namespace GameScene.Weapons
{
    public class Glock : BaseWeapon, SoldierClassPlayer.IWeapon, TankClassPlayer.IWeapon
    {
        public override WeaponType Type { get; } = WeaponType.Light;
        public override float Range { get; } = 25f;
        public override int Damage { get; } = 3;
        public override float Cooldown { get; } = 0.3f;
        public override int MaxAmmo { get; } = 13;
        public override float ReloadTime { get; } = 1f;
        public override int BulletsInRow { get; } = 1;
        public override float BulletsInRowSpacing { get; } = 0.1f;
        public override string Name { get; } = "Glock";
    }
}