using System;
using System.Collections.Generic;
using System.Linq;
using GameScene.Items;
using GameScene.PlayerControllers.BasePlayer;
using GameScene.Weapons;
using UnityEngine;
using UnityEngine.UI;

namespace GameScene.Shop.ShopUI
{
    public class ShopUI : MonoBehaviour
    {
        [SerializeField] protected GameObject weaponsShop;
        [SerializeField] protected GameObject itemsShop;
        [SerializeField] protected GameObject shopContainer;
        [SerializeField] protected GameObject shopName;
        [SerializeField] protected GameObject textMoney;
        [SerializeField] protected GameObject buyButton;
        [SerializeField] protected GameObject image;
        [SerializeField] protected GameObject holderWeapons;
        [SerializeField] protected GameObject holderItems;

        private GameObject shop;
        private GameObject containerWeaponsObj;
        private GameObject containerItemsObj;
        private Text moneyText;
        private Text Name;
        private Button showWeaponsButton;
        private Button showItemsButton;
        
        private List<BaseWeapon> weapons;
        private List<BaseItem> items;
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

        public void Init(List<BaseWeapon> weapons, List<BaseItem> items, BasePlayer player, Transform HUD)
        {
            if(weapons == null || items == null)
                return;
            Cursor.lockState = CursorLockMode.Confined;
            shop = Instantiate(gameObject, HUD);
            Name = Instantiate(shopName, shop.transform).GetComponent<Text>();
            moneyText = Instantiate(textMoney, shop.transform).GetComponent<Text>();
            showWeaponsButton = Instantiate(weaponsShop, shop.transform).GetComponent<Button>();
            showItemsButton = Instantiate(itemsShop, shop.transform).GetComponent<Button>();
            this.player = player;
            this.weapons = weapons;
            this.items = items;
            

            Name.GetComponent<Text>().text = "Shop";
            moneyText.GetComponent<Text>().text = $"Player's money : {this.player.Money}";
            showWeaponsButton.interactable = true;
            showItemsButton.interactable = true;
            showWeaponsButton.onClick.AddListener(showWeapons);
            showItemsButton.onClick.AddListener(showItems);
            containerWeaponsObj = Instantiate(shopContainer, shop.transform);
            ShopContainer containerWeapons = containerWeaponsObj.GetComponent<ShopContainer>();
            containerWeapons.Init(weapons, items, holderWeapons, holderItems, buyButton, image, true);
            
            containerItemsObj = Instantiate(shopContainer, shop.transform);
            ShopContainer containerItems = containerItemsObj.GetComponent<ShopContainer>();
            containerItems.Init(this.weapons, this.items, holderWeapons, holderItems, buyButton, image, false);
            
            containerItemsObj.SetActive(false);
            containerWeaponsObj.SetActive(false);
        }
        public void Update()
        {
            if(weapons == null || items == null)
                return;
            
            moneyText.GetComponent<Text>().text = $"Player's money : {player.Money}$";
        }

        private void showWeapons()
        {
            containerItemsObj.SetActive(false);
            containerWeaponsObj.SetActive(true);
        }

        private void showItems()
        {
            containerItemsObj.SetActive(true);
            containerWeaponsObj.SetActive(false);
        }
    }
}