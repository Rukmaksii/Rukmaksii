using GameScene.PlayerControllers.BasePlayer;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace GameScene.Shop.ShopUI
{
    [RequireComponent(typeof(VerticalLayoutGroup))]
    public abstract class Holder:MonoBehaviour
    {
        [SerializeField] protected GameObject holderName;
        protected GameObject image;
        protected GameObject buyButton;
        
        protected BasePlayer player;
        
        public abstract void CanBuy(bool canBuy);
        protected abstract void Buy();
    }
}