using System;
using GameScene.PlayerControllers.BasePlayer;
using GameScene.model;
using Unity.Netcode;
using UnityEngine;

namespace GameScene.Shop
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(NetworkObject))]
    public class ShopController : NetworkBehaviour, IPickable
    {
        [SerializeField] public ShopUI.ShopUI ShopUI;
        public void PickUp(BasePlayer player)
        {
            player.OpenShop(this);
        }

        public void Drop()
        {
            throw new InvalidOperationException();
        }

        public bool IsOwned { get; }
    }
}