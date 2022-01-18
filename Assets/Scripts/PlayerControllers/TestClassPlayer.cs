namespace PlayerControllers
{
    public class TestClassPlayer : BasePlayer
    {
        public override string ClassName { get; } = "test class";
        protected override int maxHealth { get; } = 100;

        protected override float movementSpeed { get; } = 30f;
    }
}