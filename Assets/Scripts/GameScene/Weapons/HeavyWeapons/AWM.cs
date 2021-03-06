using GameScene.model;
using GameScene.PlayerControllers;

namespace GameScene.Weapons.HeavyWeapons
{
    public class AWM : BaseWeapon, ScoutClassPlayer.IWeapon
    {
        public override WeaponType Type { get; } = WeaponType.Heavy;
        public override float Range { get; } = 200f;
        public override int Damage { get; } = 60;
        public override float Cooldown { get; } = 1f;
        public override int MaxAmmo { get; } = 5;
        public override float ReloadTime { get; } = 2f;
        public override int BulletsInRow { get; } = 1;
        public override float BulletsInRowSpacing { get; } = 0.1f;
        public override int Price { get; } = 1500;
        public override string Name { get; } = "AWM";
    }
}