using model;
using PlayerControllers;
using UnityEngine;

namespace Weapons
{
    public class TestAutoGun : BaseWeapon, SoldierClassPlayer.IWeapon
    {
        public override WeaponType Type { get; } = WeaponType.Heavy;
        public override float Range { get; } = 100f;
        public override int Damage { get; } = 5;
        public override float Cooldown { get; } = 0.1f;
        public override int MaxAmmo { get; } = 30;
        public override float ReloadTime { get; } = 5f;
        public override int BulletsInRow { get; } = 1;
        public override float BulletsInRowSpacing { get; } = 0.1f;
        public override string Name { get; } = "auto-rifle";
    }
}