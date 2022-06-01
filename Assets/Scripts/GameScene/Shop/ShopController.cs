using GameScene.model;
using GameScene.PlayerControllers.BasePlayer;
using Unity.Netcode;
using UnityEngine;

namespace GameScene.Shop
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(NetworkObject))]
    public class ShopController : NetworkBehaviour, IInteractable
    {
        [SerializeField] public ShopUI.ShopUI ShopUI;

        public void Interact(BasePlayer player)
        {
        }

        public void UnInteract()
        {
        }

        public bool IsOwned { get; }
        public string InteractableName { get; } = "Shop";
    }
}