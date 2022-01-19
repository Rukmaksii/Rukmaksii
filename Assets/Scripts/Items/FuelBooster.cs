using model;
using UnityEngine;

namespace Items
{
    public class FuelBooster : BaseItem
    {
        public override ItemType Type { get; } = ItemType.Passive;

        public override string Name { get; } = "FuelBooster";

        public override void InitializePassive()
        {
            Player.Jetpack.FuelDuration = 1f;
        }

        public override void RemovePassive()
        {
            Player.Jetpack.FuelDuration = Player.DefaultFuelDuration;
        }

        public override  void Consume()
        {
            // not a consumable
        }

        public override void Use()
        {
            // not a used item
        }
    }
}