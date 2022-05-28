using System;
using System.Linq;
using GameScene.GameManagers;
using GameScene.PlayerControllers.BasePlayer;
using GameScene.model;
using Unity.Netcode;
using UnityEngine;
using GameScene.Weapons;

namespace GameScene.Shop
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(NetworkObject))]
    public class ShopController : NetworkBehaviour, IPickable
    {
        public void PickUp(BasePlayer player)
        {
            var possibleWeapons = GameController.Singleton.WeaponPrefabs
                .Select(go => go.GetComponent<BaseWeapon>())
                .Where(bw => bw.GetType().GetInterfaces().Contains(player.WeaponInterface))
                .ToList();
            possibleWeapons.ForEach(Debug.Log);
        }

        public void Drop()
        {
            throw new InvalidOperationException();
        }

        public bool IsOwned { get; }
    }
}