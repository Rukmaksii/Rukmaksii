using ExitGames.Client.Photon.StructWrapping;
using GameScene.Weapons;
using Unity.Netcode;
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
            float imageRatio =  this.weapon.Sprite.rect.height / this.weapon.Sprite.rect.width;
            this.image.GetComponent<Image>().transform.localScale = new Vector3(1, imageRatio);
            this.image.GetComponent<Image>().sprite = this.weapon.Sprite;
            this.buyButton.GetComponentInChildren<Text>().text = $"Buy: {this.weapon.Price}$";
            CanBuy(true);
        }

        public override void CanBuy(bool canBuy)
        {
            btn.interactable = btn.enabled = canBuy;
            btn.GetComponent<Image>().color = canBuy ? Color.white : Color.gray;
        }

        protected override void Buy()
        {
            GameObject baseWeapon = Instantiate(weapon.gameObject);
            baseWeapon.GetComponent<NetworkObject>().Spawn();
            player.Inventory.AddWeapon(baseWeapon.GetComponent<BaseWeapon>());
            player.UpdateMoneyServerRpc(player.Money - weapon.Price);
        }
    }
}