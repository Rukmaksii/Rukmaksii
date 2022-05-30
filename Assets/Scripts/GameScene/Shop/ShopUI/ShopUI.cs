using System;
using System.Collections.Generic;
using GameScene.Items;
using GameScene.PlayerControllers.BasePlayer;
using GameScene.Weapons;
using UnityEngine;
using UnityEngine.UI;

namespace GameScene.Shop.ShopUI
{
    public class ShopUI : MonoBehaviour
    {
        [SerializeField] protected GameObject shop;
        [SerializeField] protected GameObject weaponsShop;
        [SerializeField] protected GameObject itemsShop;
        [SerializeField] protected GameObject shopContainer;
        [SerializeField] protected GameObject shopName;
        [SerializeField] protected GameObject textMoney;
        [SerializeField] protected GameObject buyButton;
        [SerializeField] protected GameObject image;
        [SerializeField] protected GameObject holderWeapons;
        [SerializeField] protected GameObject holderItems;

        private List<BaseWeapon> weapons;
        private List<BaseItem> items;

        private BasePlayer player;
        private GameObject moneyText;
        
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

        public void Init(List<BaseWeapon> weapons, List<BaseItem> items, BasePlayer player, Transform HUD)
        {
            if(weapons == null || items == null)
                return;
            Cursor.lockState = CursorLockMode.Confined;
            shop = Instantiate(gameObject, HUD);
            Instantiate(shopName, shop.transform);
            moneyText = Instantiate(textMoney, shop.transform);
            Instantiate(weaponsShop, shop.transform).GetComponent<Button>().interactable = true;
            Instantiate(itemsShop, shop.transform).GetComponent<Button>().interactable = true;
            this.player = player;
            this.weapons = weapons;
            this.items = items;
            

            shopName.GetComponent<Text>().text = "Shop";
            weaponsShop.GetComponent<Button>().onClick.AddListener(showWeapons);
            itemsShop.GetComponent<Button>().onClick.AddListener(showItems);
            showWeapons();
        }
        public void Update()
        {
            if(weapons == null || items == null)
                return;
            
            moneyText.GetComponent<Text>().text = $"Player's money : {player.Money}$";
        }

        private void showWeapons()
        {
            GameObject containerObj = Instantiate(shopContainer, shop.transform);
            ShopContainer container = containerObj.GetComponent<ShopContainer>();
            container.Init(weapons, items, holderWeapons, holderItems, buyButton, image, containerObj, true);
        }

        private void showItems()
        {
            GameObject containerObj = Instantiate(shopContainer, shop.transform);
            ShopContainer container = containerObj.GetComponent<ShopContainer>();
            container.Init(weapons, items, holderWeapons, holderItems, buyButton, image, containerObj, false);
        }
    }
}