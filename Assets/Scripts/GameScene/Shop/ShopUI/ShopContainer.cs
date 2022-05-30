using System.Collections.Generic;
using GameScene.GameManagers;
using GameScene.Items;
using GameScene.PlayerControllers.BasePlayer;
using GameScene.Weapons;
using UnityEngine;
using UnityEngine.UI;

namespace GameScene.Shop.ShopUI
{
    [RequireComponent(typeof(HorizontalLayoutGroup))]
    public class ShopContainer : MonoBehaviour
    {
        private GameObject containerObj;
        private GameObject holderWeapons;
        private GameObject holderItems;
        private GameObject buyButton;
        private GameObject image;
        private List<BaseWeapon> listWeapons;
        private List<BaseItem> listItems;
        private List<Holder> listBuyable;
        private BasePlayer player;

        public void Init(List<BaseWeapon> listweapons, List<BaseItem> listitems, GameObject holderWeaponsObj, GameObject holderItemsObj,
            GameObject buyButton, GameObject image, GameObject containerObj ,bool IsWeapons)
        {
            player = GameController.Singleton.LocalPlayer;
            this.image = image;
            this.buyButton = buyButton;
            this.containerObj = containerObj;
            listItems = listitems;
            listWeapons = listweapons;
            if (IsWeapons)
            {
                this.holderWeapons = holderWeaponsObj;
                foreach (BaseWeapon weapon in listWeapons)
                {
                    HolderWeapons holderWeapons =
                        Instantiate(this.holderWeapons, this.containerObj.transform).GetComponent<HolderWeapons>();
                    holderWeapons.Init(weapon, this.buyButton, this.image);
                }
            }
            else
            {
                this.holderItems = holderItemsObj;
                foreach (BaseItem item in listItems)
                {
                    HolderItems holderItems =
                        Instantiate(this.holderItems, this.containerObj.transform).GetComponent<HolderItems>();
                    holderItems.Init(item, this.buyButton, this.image);
                }
            }
        }

        private void Update()
        {
            if (listWeapons == null)
                return;
            
            foreach (HolderItems holderItems in GetComponentsInChildren<HolderItems>())
            {
                if (holderItems.item.Price > player.Money)
                    holderItems.CanBuy(false);
                else
                    holderItems.CanBuy(true);
            }
            foreach (HolderWeapons holderWeapons in GetComponentsInChildren<HolderWeapons>())
            {
                if (holderWeapons.weapon.Price > player.Money)
                    holderWeapons.CanBuy(false);
                else
                    holderWeapons.CanBuy(true);
            }
        }
    }
}