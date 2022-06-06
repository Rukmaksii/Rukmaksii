using GameScene.model;
using GameScene.PlayerControllers;

namespace GameScene.Weapons.HeavyWeapons
{
    public class MP5 : BaseWeapon, ScoutClassPlayer.IWeapon
    {
        public override WeaponType Type { get; } = WeaponType.Heavy;
        public override float Range { get; } = 50;
        public override int Damage { get; } = 4;
        public override float Cooldown { get; } = 0.08f;
        public override int MaxAmmo { get; } = 20;
        public override float ReloadTime { get; } = 1f;
        public override int BulletsInRow { get; } = 1;
        public override float BulletsInRowSpacing { get; } = 0.1f;
        public override int Price { get; } = 700;
        public override string Name { get; } = "MP5";
    }
}