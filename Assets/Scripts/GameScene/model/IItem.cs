namespace GameScene.model
{
    public enum ItemCategory
    {
        Attack,
        Heal,
        Defense,
        Other
    }

    public enum ItemState
    {
        Clean, 
        Consuming,
        Consumed
    }

    public interface IItem
    {
        void Consume();
    }
}