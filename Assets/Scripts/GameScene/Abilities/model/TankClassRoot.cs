using System;
using System.Collections.Generic;

namespace GameScene.Abilities.model
{
    public class TankClassRoot : RootAbility
    {
        public override List<Type> Children { get; } = new List<Type>
        {
            typeof(HealthAbility),
            typeof(DamageMultiplierAbility)
        };
    }
}