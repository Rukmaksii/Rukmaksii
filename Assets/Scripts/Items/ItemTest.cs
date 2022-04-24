namespace Items
{
    public class ItemTest : BaseItem
    {
        public override float Duration { get; protected set; } = 3f;

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