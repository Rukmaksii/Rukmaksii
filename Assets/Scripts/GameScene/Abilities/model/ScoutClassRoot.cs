using System;
using System.Collections.Generic;

namespace GameScene.Abilities.model
{
    public class ScoutClassRoot : RootAbility
    {
        public override List<Type> Children { get; } = new List<Type>
        {
            typeof(JetpackAbility)
        };
    }
}