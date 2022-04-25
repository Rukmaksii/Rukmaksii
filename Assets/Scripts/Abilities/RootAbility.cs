using PlayerControllers;

namespace Abilities
{
    public abstract class RootAbility : BaseAbility
    {
        protected RootAbility(BasePlayer player) : base(player)
        {
        }

        public override void Apply()
        {
        }
    }
}