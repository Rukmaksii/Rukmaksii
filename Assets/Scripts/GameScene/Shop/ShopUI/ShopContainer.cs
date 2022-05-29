using System.Collections.Generic;
using GameScene.model;
using UnityEngine;
using UnityEngine.UI;

namespace GameScene.Shop.ShopUI
{
    [RequireComponent(typeof(HorizontalLayoutGroup))]
    public class ShopContainer : MonoBehaviour
    {
        private List<Holder> listBuyable;

        public void Init(List<Holder> listBuyable)
        {
            this.listBuyable = listBuyable;
            UpdateDisplay();
        }

        private void UpdateDisplay()
        {
            foreach (Holder holder in listBuyable)
            {
                Instantiate(holder, this.transform);
            }
        }
    }
}