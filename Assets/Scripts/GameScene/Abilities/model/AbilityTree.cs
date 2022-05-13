using System;
using System.Collections.Generic;
using GameScene.PlayerControllers.BasePlayer;

namespace GameScene.Abilities.model
{
    public class AbilityTree
    {
        private readonly BasePlayer player;
        public BaseAbility CurrentAbility { get; private set; }
        public readonly List<BaseAbility> Abilities = new List<BaseAbility>();

        public AbilityTree(BasePlayer player, BaseAbility currentAbility)
        {
            this.player = player;
            CurrentAbility = currentAbility;
        }

        public void ChooseAbility(Type t)
        {
            if (CurrentAbility == null)
                return;
        }

        private void InstantiateAbility(Type t)
        {
            CurrentAbility = (BaseAbility) t.GetConstructor(new Type[] {typeof(BasePlayer)})!.Invoke(new object[] {player});
            CurrentAbility.Apply();
        }
    }
}