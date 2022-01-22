namespace model
{

    public enum ItemType
    {
        Consumable,
        Active,
        Passive
    }
    
    
    public interface IItem
    {
        public void Start();
        public void Update();
        public void OnDestroy();
    }

    public interface IIActiveItem : IItem
    {
        public void Use();
    }
}