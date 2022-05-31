using System;
using GameScene.PlayerControllers.BasePlayer;
using GameScene.model;
using Photon.Realtime;
using Unity.Netcode;
using UnityEngine;

namespace GameScene.Shop
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(NetworkObject))]
    public class ShopController : NetworkBehaviour
    {
        [SerializeField] public ShopUI.ShopUI ShopUI;
        public void PickUp(BasePlayer player)
        {
            
        }

        public void Drop()
        {
        }

        public bool IsOwned { get; }
    }
}