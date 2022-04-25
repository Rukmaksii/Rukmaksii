using System;
using System.Collections.Generic;
using model;
using PlayerControllers;

namespace Abilities
{
    public abstract class BaseAbility : IAbility
    {
        protected BasePlayer Player;
        public abstract string Name { get; }

        public abstract List<Type> ChildrenAbilities { get; }


        public BaseAbility(BasePlayer player)
        {
            Player = player;
        }

        public abstract void Apply();
    }
}