using GameScene.model;
using GameScene.PlayerControllers;

namespace GameScene.Weapons
{
    public class Deagle : BaseWeapon, SoldierClassPlayer.IWeapon, TankClassPlayer.IWeapon
    {
        public override WeaponType Type { get; } = WeaponType.Light;
        public override float Range { get; } = 50f;
        public override int Damage { get; } = 7;
        public override float Cooldown { get; } = 0.7f;
        public override int MaxAmmo { get; } = 7;
        public override float ReloadTime { get; } = 2f;
        public override int BulletsInRow { get; } = 1;
        public override float BulletsInRowSpacing { get; } = 0.1f;
        public override string Name { get; } = "Deagle";
    }
}