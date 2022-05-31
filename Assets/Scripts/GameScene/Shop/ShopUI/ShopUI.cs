using System;
using System.Collections.Generic;
using System.Linq;
using GameScene.Items;
using GameScene.PlayerControllers.BasePlayer;
using GameScene.Weapons;
using Photon.Realtime;
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
        private GameObject moneyText;
        private Text textShop;
        private Button showWeaponsButton;
        private Button showItemsButton;
        
        private List<BaseWeapon> weapons;
        private List<BaseItem> items;
        private BasePlayer player;

        public GameObject Init(List<BaseWeapon> weapons, List<BaseItem> items, BasePlayer player, Transform HUD)
        {
            if(weapons == null || items == null)
                return null;
            shop = Instantiate(gameObject, HUD);
            textShop = Instantiate(shopName, shop.transform).GetComponent<Text>();
            moneyText = Instantiate(textMoney, shop.transform);
            showWeaponsButton = Instantiate(weaponsShop, shop.transform).GetComponent<Button>();
            showItemsButton = Instantiate(itemsShop, shop.transform).GetComponent<Button>();
            this.player = player;
            this.weapons = weapons;
            this.items = items;
            

            textShop.text = "Shop";
            moneyText.GetComponent<Text>().text = "";
            
            showWeaponsButton.interactable = true;
            showItemsButton.interactable = true;
            showWeaponsButton.onClick.AddListener(ShowWeapons);
            showItemsButton.onClick.AddListener(ShowItems);
            
            containerWeaponsObj = Instantiate(shopContainer, shop.transform);
            ShopContainer containerWeapons = containerWeaponsObj.GetComponent<ShopContainer>();
            containerWeapons.Init(weapons, items, holderWeapons, holderItems, buyButton, image, true);
            
            containerItemsObj = Instantiate(shopContainer, shop.transform);
            ShopContainer containerItems = containerItemsObj.GetComponent<ShopContainer>();
            containerItems.Init(this.weapons, this.items, holderWeapons, holderItems, buyButton, image, false);
            
            containerItemsObj.SetActive(false);
            containerWeaponsObj.SetActive(true);
            return shop;
        }

        private void ShowWeapons()
        {
            containerItemsObj.SetActive(false);
            containerWeaponsObj.SetActive(true);
        }

        private void ShowItems()
        {
            containerItemsObj.SetActive(true);
            containerWeaponsObj.SetActive(false);
        }
    }
}