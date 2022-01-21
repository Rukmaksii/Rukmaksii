using model;

namespace Weapons
{
    public class TestRifleGun : BaseWeapon
    {
        public override WeaponType Type { get; } = WeaponType.Heavy;
        public override float Range { get; } = 100f;
        public override int Damage { get; } = 3;
        public override float Cooldown { get; } = 1f;
        public override int MaxAmmo { get; } = 50;
        public override float ReloadTime { get; } = 5f;
        public override int BulletsInRow { get; } = 5;
        public override float BulletsInRowSpacing { get; } = 0.1f;
    }
}