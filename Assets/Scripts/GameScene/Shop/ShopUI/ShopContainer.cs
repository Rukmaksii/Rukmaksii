using System.Collections.Generic;
using GameScene.GameManagers;
using GameScene.Items;
using GameScene.PlayerControllers.BasePlayer;
using GameScene.Weapons;
using UnityEngine;
using UnityEngine.UI;

namespace GameScene.Shop.ShopUI
{
    public class ShopContainer : MonoBehaviour
    {
        private GameObject holderWeapons;
        private GameObject holderItems;
        private GameObject buyButton;
        private GameObject image;
        private List<BaseWeapon> listWeapons;
        private List<BaseItem> listItems;
        private List<HolderWeapons> holderWeaponsList = new List<HolderWeapons>();
        private List<HolderItems> holderItemsList = new List<HolderItems>();
        private BasePlayer player;
        public bool IsWeapons { get; private set; }

        public void Init(List<BaseWeapon> listweapons, List<BaseItem> listitems, GameObject holderWeaponsObj, GameObject holderItemsObj,
            GameObject buyButton, GameObject image,bool IsWeapons)
        {
            player = GameController.Singleton.LocalPlayer;
            this.image = image;
            this.buyButton = buyButton;
            listItems = listitems;
            listWeapons = listweapons;
            this.IsWeapons = IsWeapons;
            if (IsWeapons)
            {
                this.holderWeapons = holderWeaponsObj;
                holderWeaponsList = new List<HolderWeapons>();
                foreach (BaseWeapon weapon in listWeapons)
                {
                    HolderWeapons holderWeapons =
                        Instantiate(this.holderWeapons, transform).GetComponent<HolderWeapons>();
                    holderWeapons.Init(weapon, this.buyButton, this.image);
                    holderWeaponsList.Add(holderWeapons);
                }
            }
            else
            {
                this.holderItems = holderItemsObj;
                holderItemsList = new List<HolderItems>();
                foreach (BaseItem item in listItems)
                {
                    HolderItems holderItems =
                        Instantiate(this.holderItems, transform).GetComponent<HolderItems>();
                    holderItems.Init(item, this.buyButton, this.image);
                    holderItemsList.Add(holderItems);
                }
            }
        }

        private void Update()
        {
            if (listWeapons == null)
                return;
            if (IsWeapons)
            {
                foreach (HolderWeapons holder in holderWeaponsList)
                {
                    if (holder.weapon.Price < player.Money)
                    {
                        holder.CanBuy(true);
                    }
                    else
                    {
                        holder.CanBuy(false);
                    }
                }
            }
            else
            {
                foreach (HolderItems holder in holderItemsList)
                {
                    if (holder.item.Price < player.Money)
                    {
                        holder.CanBuy(true);
                    }
                    else
                    {
                        holder.CanBuy(false);
                    }
                }
            }
        }

        public void Deactivate()
        {
            foreach (HolderItems items in holderItemsList)
            {
                Destroy(items.gameObject);
            }

            foreach (HolderWeapons weapons in holderWeaponsList)
            {
                Destroy(weapons.gameObject);
            }
        }
    }
}