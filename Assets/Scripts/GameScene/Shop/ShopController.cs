using System;
using System.Linq;
using GameManagers;
using model;
using PlayerControllers;
using Unity.Netcode;
using UnityEngine;
using Weapons;

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