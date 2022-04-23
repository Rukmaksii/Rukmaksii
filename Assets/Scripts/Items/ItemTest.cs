namespace Items
{
    public class ItemTest : BaseItem
    {
        public override float Duration { get; protected set; } = 3f;

        // storing the old fuel value
        private float oldFuelValue = 10f;

        private float newFuelValue;

        protected override void Setup()
        {
        }

        protected override void OnConsume()
        {
        }

        protected override void TearDown()
        {
        }
    }
}