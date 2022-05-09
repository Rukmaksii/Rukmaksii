using Abilities;

namespace PlayerControllers
{
    public class TestClassPlayer : BasePlayer
    {
        public override string ClassName { get; } = "test class";
        public override int MaxHealth { get; protected set; } = 100;
        public override RootAbility RootAbility { get; } = new TestClassRoot();
        protected override float movementSpeed { get; } = 10f;
    }
}