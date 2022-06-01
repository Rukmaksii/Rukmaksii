using GameScene.PlayerControllers.BasePlayer;

namespace GameScene.model
{
    public interface IInteractable
    {
        
        /**
         * <param name="player">the player that interacts with the object</param>
         * <returns>true if interaction has been made</returns>
         */
        bool Interact(BasePlayer player);

        void UnInteract();

        public bool IsOwned { get; }

        string InteractableName { get; }
    }
}