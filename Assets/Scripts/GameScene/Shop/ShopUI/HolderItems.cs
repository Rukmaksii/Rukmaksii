using GameScene.Items;
using UnityEngine;
using UnityEngine.UI;

namespace GameScene.Shop.ShopUI
{
    public class HolderItems: Holder
    {
        public BaseItem item { get; private set; }
        protected Button btn;
        
        public void Init(BaseItem item, GameObject buyButton, GameObject image)
        {
            player = GameManagers.GameController.Singleton.LocalPlayer;
            this.item = item;
            this.buyButton = buyButton;
            this.image = image;
            btn = this.buyButton.GetComponent<Button>();
            btn.onClick.AddListener(Buy);
            buttonColor = btn.colors;
            buttonColor.normalColor = Color.yellow;
            buttonColor.highlightedColor = Color.yellow;
            buttonColor.pressedColor = Color.green;
            UpdateUI();
        }

        protected override void UpdateUI()
        {
            image.GetComponent<Image>().sprite = item.Info.Sprite;
            buyButton.GetComponentInChildren<Text>().text = $"Buy: {item.Price}$";
        }

        public override void CanBuy(bool canbuy)
        {
            btn.interactable = canbuy;
            if (canbuy)
            {
                buttonColor.normalColor = Color.yellow;
                buttonColor.highlightedColor = Color.yellow;
                buttonColor.pressedColor = Color.green;
            }
            else
            {
                buttonColor.normalColor = Color.gray;
                buttonColor.highlightedColor = Color.gray;
                buttonColor.pressedColor = Color.gray;
            }
        }

        protected override void Buy()
        {
            throw new System.NotImplementedException();
        }
    }
}