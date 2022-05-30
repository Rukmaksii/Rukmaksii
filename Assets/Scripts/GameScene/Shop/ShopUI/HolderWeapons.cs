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
            btn.interactable = true;
            buttonColor = btn.colors;
            buttonColor.normalColor = Color.yellow;
            buttonColor.highlightedColor = Color.yellow;
            buttonColor.pressedColor = Color.green;
            UpdateUI();
        }

        protected override void UpdateUI()
        {
            float imageratio =  weapon.Sprite.rect.width / image.GetComponent<Image>().sprite.rect.width;
            weapon.Sprite.texture.Resize((int)(weapon.Sprite.rect.width * imageratio), (int)(weapon.Sprite.rect.height * imageratio));
            image.GetComponent<Image>().sprite = weapon.Sprite;
            buyButton.GetComponentInChildren<Text>().text = $"Buy: {weapon.Price}$";
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