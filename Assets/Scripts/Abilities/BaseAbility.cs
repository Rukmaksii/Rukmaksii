using System.Collections.Generic;
using model;
using PlayerControllers;

namespace Ability
{
    public abstract class BaseAbility : IAbility
    {
        protected BasePlayer player;
        public virtual string name { get; }

        public List<BaseAbility> childrenAbilities { get; private set; } = new List<BaseAbility>();


        protected BaseAbility(BasePlayer player)
        {
            this.player = player;
        }

        public abstract void Apply();
    }
}