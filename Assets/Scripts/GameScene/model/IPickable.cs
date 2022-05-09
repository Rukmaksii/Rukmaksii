using PlayerControllers;

namespace model
{
    public interface IPickable
    {
        void PickUp(BasePlayer player);

        void Drop();

        public bool IsOwned { get; }
    }
}