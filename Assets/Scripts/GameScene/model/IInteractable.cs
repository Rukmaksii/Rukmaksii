using GameScene.PlayerControllers.BasePlayer;

namespace GameScene.model
{
    public interface IInteractable
    {
        void Interact(BasePlayer player);

        void UnInteract();

        public bool IsOwned { get; }

        string InteractableName { get; }
    }
}