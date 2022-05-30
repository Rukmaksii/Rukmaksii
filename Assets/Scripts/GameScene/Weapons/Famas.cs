using GameScene.model;
using GameScene.PlayerControllers;

namespace GameScene.Weapons
{
    public class Famas : BaseWeapon, SoldierClassPlayer.IWeapon
    {
        public override WeaponType Type { get; } = WeaponType.Heavy;
        public override float Range { get; } = 100f;
        public override int Damage { get; } = 4;
        public override float Cooldown { get; } = 0.45f;
        public override int MaxAmmo { get; } = 27;
        public override float ReloadTime { get; } = 1.5f;
        public override int BulletsInRow { get; } = 3;
        public override float BulletsInRowSpacing { get; } = 0.1f;
        public override string Name { get; } = "Famas";
    }
}