using System;
using System.Collections.Generic;
using GameScene.Items;
using GameScene.PlayerControllers.BasePlayer;
using GameScene.Weapons;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GameScene.Shop.ShopUI
{
    public class ShopUI : MonoBehaviour
    {
        [SerializeField] protected Canvas shop;
        [SerializeField] protected Button weaponsShop;
        [SerializeField] protected Button itemsShop;
        [SerializeField] protected Canvas shopContainer;
        [SerializeField] protected Text shopName;
        [SerializeField] protected Text textMoney;

        private List<BaseWeapon> weapons;
        private List<BaseItem> items;
        private List<Holder> weaponsHolder;
        private List<Holder> itemsHolder;

        private BasePlayer player;
        
        public static ShopUI Singleton { get; private set; }

        private void Awake()
        {
            if (Singleton != null && Singleton != this)
            {
                Destroy(this);
                return;
            }

            Singleton = this;
        }

        public void Init(List<BaseWeapon> weapons, List<BaseItem> items, BasePlayer player)
        {
            this.player = player;
            this.weapons = weapons;
            this.items = items;
            foreach (BaseWeapon weapon in weapons)
            {
                Holder holder = new Holder();
                holder.Init(weapon);
                weaponsHolder.Add(holder);
            }
            foreach (BaseItem item in items)
            {
                Holder holder = new Holder();
                holder.Init(weapons[0]);
                weaponsHolder.Add(holder);
            }

            shopName.text = "Shop";
            
        }
        public void Update()
        {
            if(weapons == null || items == null)
                return;
            textMoney.text = $"Player's money : {player.Money}$";

            foreach (Holder holder in itemsHolder)
            {
                if (holder.weapon.Price > player.Money)
                    holder.CanBuy(false);
                else
                    holder.CanBuy(true);
            }
            foreach (Holder holder in weaponsHolder)
            {
                if (holder.weapon.Price > player.Money)
                    holder.CanBuy(false);
                else
                    holder.CanBuy(true);
            }
            //When weaponsShop is clicked showWeapons()
            
            //When itemsShop is clicked showItems()
        }

        private void showWeapons()
        {
            ShopContainer container = Instantiate(shopContainer, this.transform).GetComponent<ShopContainer>();
            container.Init(weaponsHolder);
        }

        private void showItems()
        {
            ShopContainer container = Instantiate(shopContainer, this.transform).GetComponent<ShopContainer>();
            container.Init(itemsHolder);
        }
    }
}