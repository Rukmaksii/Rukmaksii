using model;
using UnityEngine;

namespace Items
{
    public class FuelBooster : BaseItem
    {
        public override ItemType Type { get; } = ItemType.Passive;

        public override string Name { get; } = "FuelBooster";
        
        
    }
}