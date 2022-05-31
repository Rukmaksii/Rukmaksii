using ExitGames.Client.Photon.StructWrapping;
using GameScene.Weapons;
using UnityEngine;
using UnityEngine.UI;

namespace GameScene.Shop.ShopUI
{
    public class HolderWeapons : Holder
    {
        public BaseWeapon weapon { get; private set; }
        protected Button btn;

        public void Init(BaseWeapon weapon, GameObject buyButton, GameObject image)
        {
            player = GameManagers.GameController.Singleton.LocalPlayer;
            this.weapon = weapon;
            this.image = Instantiate(image, gameObject.transform);
            this.buyButton = Instantiate(buyButton, gameObject.transform);
            btn = this.buyButton.GetComponent<Button>();
            btn.onClick.AddListener(Buy);
            buttonColor = btn.colors;
            buttonColor.normalColor = Color.yellow;
            buttonColor.highlightedColor = Color.yellow;
            buttonColor.pressedColor = Color.green;
            buttonColor.disabledColor = Color.gray;
            float imageRatio =  this.weapon.Sprite.rect.height / this.weapon.Sprite.rect.width;
            this.image.GetComponent<Image>().transform.localScale = new Vector3(1, imageRatio);
            this.image.GetComponent<Image>().sprite = this.weapon.Sprite;
            this.buyButton.GetComponentInChildren<Text>().text = $"Buy: {this.weapon.Price}$";
            CanBuy(true);
        }

        public override void CanBuy(bool canBuy)
        {
            btn.interactable = canBuy;
            btn.enabled = canBuy;
        }

        protected override void Buy()
        {
            throw new System.NotImplementedException();
        }
    }
}