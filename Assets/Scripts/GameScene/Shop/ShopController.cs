using System.Collections.Generic;
using System.Linq;
using GameScene.GameManagers;
using GameScene.HUD;
using GameScene.Items;
using GameScene.Map;
using GameScene.model;
using GameScene.PlayerControllers.BasePlayer;
using GameScene.Weapons;
using Unity.Netcode;
using UnityEngine;

namespace GameScene.Shop
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(NetworkObject))]
    public class ShopController : NetworkBehaviour, IInteractable
    {
        [SerializeField] public ShopUI.ShopUI ShopUI;
        [SerializeField] private ObjectiveController objectiveController;

        public bool Interact(BasePlayer player)
        {
            List<BaseWeapon> possibleWeapons = GameController.Singleton.WeaponPrefabs
                .Select(go => go.GetComponent<BaseWeapon>())
                .Where(bw => bw.GetType().GetInterfaces().Contains(player.WeaponInterface))
                .ToList();
            List<BaseItem> possibleItems =
                GameController.Singleton.ItemPrefabs
                    .Select(go => go.GetComponent<BaseItem>())
                    .ToList();
            HUDController.Singleton.ShopUI.Init(possibleWeapons, possibleItems, player);

            return true;
        }

        public void UnInteract()
        {
            HUDController.Singleton.ShopUI.Hide();
        }

        public bool IsInteractable =>
            !(objectiveController.CurrentState == ObjectiveController.State.Captured &&
            GameController.Singleton.LocalPlayer.TeamId == objectiveController.ControllingTeam);

        public string InteractableName { get; } = "Shop";
    }
}