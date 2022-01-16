using model;
using UnityEngine;

namespace Weapons
{
    public class TestGun : BaseWeapon
    {
        public override WeaponType Type { get; } = WeaponType.Light;
        public override float Range { get; } = 50f;
        public override int Damage { get; } = 5;
        public override float Cooldown { get; } = 1f;
        public override int MaxAmmo { get; } = 10;
        public override float ReloadTime { get; } = 3f;
        public override int BulletsInRow { get; } = 1;
        public override float BulletsInRowSpacing { get; } = 0;
    }
}