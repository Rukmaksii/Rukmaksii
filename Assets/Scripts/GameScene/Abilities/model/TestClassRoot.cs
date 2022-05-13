using System;
using System.Collections.Generic;

namespace GameScene.Abilities.model
{
    public class TestClassRoot : RootAbility
    {
        public override List<Type> Children { get; } = new List<Type>
        {
            typeof(JetpackAbility)
        };
    }
}