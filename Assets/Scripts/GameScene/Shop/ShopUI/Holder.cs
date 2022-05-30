using GameScene.PlayerControllers.BasePlayer;
using UnityEngine;
using UnityEngine.UI;

namespace GameScene.Shop.ShopUI
{
    [RequireComponent(typeof(VerticalLayoutGroup))]
    public abstract class Holder:MonoBehaviour
    {
        protected GameObject image;
        protected GameObject buyButton;
        protected ColorBlock buttonColor;

        protected BasePlayer player;
        
        public abstract void CanBuy(bool flag);
        protected abstract void UpdateUI();
        protected abstract void Buy();
    }
}