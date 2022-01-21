namespace model
{

    public interface IItem
    {
        public void OnStartPassive();

        public void OnPassiveCalled();
        public void OnRemovePassive();
    }

    public interface IActiveItem
    {
        public bool Use();
    }
}