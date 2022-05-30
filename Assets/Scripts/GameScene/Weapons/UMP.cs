using GameScene.model;
using GameScene.PlayerControllers;

namespace GameScene.Weapons
{
    public class UMP : BaseWeapon, SoldierClassPlayer.IWeapon, TankClassPlayer.IWeapon
    {
        public override WeaponType Type { get; } = WeaponType.Heavy;
        public override float Range { get; } = 50;
        public override int Damage { get; } = 3;
        public override float Cooldown { get; } = 0.1f;
        public override int MaxAmmo { get; } = 25;
        public override float ReloadTime { get; } = 1.5f;
        public override int BulletsInRow { get; } = 1;
        public override float BulletsInRowSpacing { get; } = 0.1f;
        public override string Name { get; } = "UMP";
    }
}