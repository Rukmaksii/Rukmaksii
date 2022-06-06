using GameScene.model;
using GameScene.PlayerControllers;

namespace GameScene.Weapons.CloseRangeWeapons
{
    public class Dagger : BaseWeapon, ScoutClassPlayer.IWeapon
    {
        public override WeaponType Type { get; } = WeaponType.CloseRange;
        public override float Range { get; } = 0.5f;
        public override int Damage { get; } = 50;
        public override string Name { get; } = "Knife";
        public override float Cooldown { get; } = 0;
        public override int MaxAmmo { get; } = 1;
        public override float ReloadTime { get; } = 0.3f;
        public override int BulletsInRow { get; } = 0;
        public override float BulletsInRowSpacing { get; } = 0;
        public override int Price { get; } = 150;
    }
}