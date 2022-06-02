using GameScene.Items;
using Unity.Netcode;
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
            Instantiate(holderName, gameObject.transform).GetComponent<Text>().text = this.item.Name;
            this.image = Instantiate(image, transform);
            this.buyButton = Instantiate(buyButton, transform);
            btn = this.buyButton.GetComponent<Button>();
            btn.onClick.AddListener(Buy);
            float imageRatio =  this.item.Info.Sprite.rect.height / this.item.Info.Sprite.rect.width;
            this.image.GetComponent<Image>().transform.localScale = new Vector3(1, imageRatio);
            this.image.GetComponent<Image>().sprite = this.item.Info.Sprite;
            this.buyButton.GetComponentInChildren<Text>().text = $"Buy: {this.item.Price}$";
            CanBuy(true);
        }
        

        public override void CanBuy(bool canBuy)
        {
            btn.interactable = btn.enabled = canBuy;
            btn.GetComponent<Image>().color = canBuy ? Color.white : Color.gray;
        }

        protected override void Buy()
        {
            GameObject baseItem = Instantiate(item.gameObject);
            baseItem.GetComponent<NetworkObject>().Spawn();
            player.Inventory.AddItem(baseItem.GetComponent<BaseItem>());
            player.UpdateMoneyServerRpc(player.Money - item.Price);
        }
    }
}