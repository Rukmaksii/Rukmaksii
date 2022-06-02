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

        public List<BaseAbility> BoughtAbilities { get; } = new List<BaseAbility>();

        public AbilityTree(BasePlayer player)
        {
            this.player = player;
            BoughtAbilities.Add(player.RootAbility);
        }

        
        public bool ChooseAbility(Type t)
        {
            if (CurrentAbility == null)
                return false;
            if (!CurrentAbility.Children.Contains(t))
                return false;

            var ab = InstantiateAbility(t);
            if (player.Money < ab.Price)
                return false;

            player.Money -= ab.Price;
            ab.Apply();
            BoughtAbilities.Add(ab);

            return true;
        }

        private BaseAbility InstantiateAbility(Type t)
        {
            return (BaseAbility) t.GetConstructor(new Type[] {typeof(BasePlayer)})!.Invoke(new object[] {player});
        }
    }
}