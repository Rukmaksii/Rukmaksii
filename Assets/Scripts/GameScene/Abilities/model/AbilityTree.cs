using System;
using System.Collections.Generic;
using System.Linq;
using GameScene.PlayerControllers.BasePlayer;

namespace GameScene.Abilities.model
{
    public class AbilityTree
    {
        private readonly BasePlayer player;
        public BaseAbility CurrentAbility => BoughtAbilities.Last();
        public readonly List<BaseAbility> Abilities = new List<BaseAbility>();

        private List<BaseAbility> BoughtAbilities { get; } = new List<BaseAbility>();

        public AbilityTree(BasePlayer player, BaseAbility currentAbility)
        {
            this.player = player;
            BoughtAbilities.Add(currentAbility);
        }

        public void ChooseAbility(Type t)
        {
            if (CurrentAbility == null)
                return;
        }

        private void InstantiateAbility(Type t)
        {
            BaseAbility next = (BaseAbility) t.GetConstructor(new Type[] {typeof(BasePlayer)})!.Invoke(new object[] {player});
            next.Apply();
            BoughtAbilities.Add(next);
        }
    }
}