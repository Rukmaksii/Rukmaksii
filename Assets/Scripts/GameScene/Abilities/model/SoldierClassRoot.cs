using System;
using System.Collections.Generic;

namespace GameScene.Abilities.model
{
    public class SoldierClassRoot : RootAbility
    {
        public override List<Type> Children { get; } = new List<Type>
        {
            typeof(DashAbility),
            typeof(PickupRangeAbility),
            typeof(SpeedAbility)
        };
    }
}