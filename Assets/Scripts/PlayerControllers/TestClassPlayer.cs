﻿namespace PlayerControllers
{
    public class TestClassPlayer : BasePlayer
    {
        public override string ClassName { get; } = "test class";
        public override int MaxHealth { get; protected set; } = 100;
        protected override float movementSpeed { get; } = 10f;
    }
}