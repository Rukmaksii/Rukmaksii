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
            this.image = Instantiate(image, transform);
            this.buyButton = Instantiate(buyButton, transform);
            btn = this.buyButton.GetComponent<Button>();
            btn.onClick.AddListener(Buy);
            buttonColor = btn.colors;
            buttonColor.normalColor = Color.yellow;
            buttonColor.highlightedColor = Color.yellow;
            buttonColor.pressedColor = Color.green;
            buttonColor.disabledColor = Color.gray;
            float imageRatio =  this.item.Info.Sprite.rect.height / this.item.Info.Sprite.rect.width;
            this.image.GetComponent<Image>().transform.localScale = new Vector3(1, imageRatio);
            this.image.GetComponent<Image>().sprite = this.item.Info.Sprite;
            this.buyButton.GetComponentInChildren<Text>().text = $"Buy: {this.item.Price}$";
            CanBuy(true);
        }
        

        public override void CanBuy(bool canbuy)
        {
            btn.interactable = canbuy;
            btn.enabled = canbuy;
        }

        protected override void Buy()
        {
            throw new System.NotImplementedException();
        }
    }
}