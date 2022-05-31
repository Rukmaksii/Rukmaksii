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
        [SerializeField] protected Button weaponsShop;
        [SerializeField] protected Button itemsShop;
        [SerializeField] private ShopContainer weaponsContainer;
        [SerializeField] private ShopContainer itemsContainer;
        [SerializeField] protected GameObject shopName;
        [SerializeField] protected GameObject textMoney;
        [SerializeField] protected GameObject buyButton;
        [SerializeField] protected GameObject image;
        [SerializeField] protected GameObject holderWeapons;
        [SerializeField] protected GameObject holderItems;


        private List<BaseWeapon> weapons;
        private List<BaseItem> items;
        private BasePlayer player;

        public void Init(List<BaseWeapon> weapons, List<BaseItem> items, BasePlayer player)
        {
            if (weapons == null || items == null)
                return;
            
            gameObject.SetActive(true);
            this.player = player;
            this.weapons = weapons;
            this.items = items;


            shopName.GetComponent<Text>().text = "Shop";
            textMoney.GetComponent<Text>().text = "";

            weaponsContainer.Init(weapons, items, holderWeapons, holderItems, buyButton, image, true);

            itemsContainer.Init(this.weapons, this.items, holderWeapons, holderItems, buyButton, image, false);
            ShowWeapons();
        }

        public void ShowWeapons()
        {
            itemsContainer.gameObject.SetActive(false);
            weaponsContainer.gameObject.SetActive(true);
        }

        public void ShowItems()
        {
            itemsContainer.gameObject.SetActive(true);
            weaponsContainer.gameObject.SetActive(false);
        }

        public void Hide()
        {
            itemsContainer.Deactivate();
            weaponsContainer.Deactivate();
        }
    }
}