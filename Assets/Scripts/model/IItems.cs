namespace model
{

    public enum ItemType
    {
        Consumable,
        Active,
        Passive
    }
    
    
    public interface IItems
    {
        public void InitializePassive();
        public void RemovePassive();
        public void Consume();
        public void Use();
    }
}