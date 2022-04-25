using System;
using System.Collections.Generic;

namespace Abilities
{
    public class TestClassRoot : RootAbility
    {
        public override List<Type> Children { get; } = new List<Type>
        {
            typeof(JetpackAbility)
        };
    }
}