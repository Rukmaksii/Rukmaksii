namespace GameScene.Items
{
    public class FuelBooster : BaseItem
    {
        public override float Duration { get; } = 3f;
        public override int Price { get; } = 100;
        protected override float ReadyCooldown { get; } = 3f;

        private float bonusFuel = 2f;


        protected override void Setup()
        {
            Player.Jetpack.FuelDuration -= bonusFuel;
        }

        protected override void OnConsume()
        {
        }

        protected override void TearDown()
        {
            Player.Jetpack.FuelDuration += bonusFuel;
        }
    }
}