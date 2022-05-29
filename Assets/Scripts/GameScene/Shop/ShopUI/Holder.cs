using System;
using System.Collections.Generic;
using GameScene.Weapons;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace GameScene.Shop.ShopUI
{
    public class Holder:MonoBehaviour
    {
        [SerializeField] protected Image image;
        [SerializeField] protected Button buyButton;

        public BaseWeapon weapon;
        
        public void Init(BaseWeapon weapon)
        {
            this.weapon = weapon;
            UpdateUI();
        }

        private void UpdateUI()
        {
            image.sprite = weapon.Sprite;
            buyButton.GetComponentInChildren<Text>().text = $"Buy: {weapon.Price}$";
        }

        public void CanBuy(bool canbuy)
        {
            if (canbuy)
            {
                buyButton.colors = ColorBlock.defaultColorBlock;
                buyButton.interactable = true;
            }
            else
            {
                //change the button's color
                buyButton.interactable = false;
            }
        }
    }
}