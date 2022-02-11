using Abilities.model;
using PlayerControllers;

namespace Ability
{
    public class TestPlayerAbility : BaseAbility, ITestPlayerAbility
    {
        public override void Apply()
        {
            // TODO implement any change due to ability
        }

        public TestPlayerAbility(BasePlayer player) : base(player)
        {
        }
    }
}