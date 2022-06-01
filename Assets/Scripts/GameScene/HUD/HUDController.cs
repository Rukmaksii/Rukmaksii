using System.Collections.Generic;
using GameScene.GameManagers;
using GameScene.Items;
using GameScene.Map;
using GameScene.model;
using GameScene.PlayerControllers.BasePlayer;
using GameScene.Shop.ShopUI;
using GameScene.Weapons;
using UnityEngine;
using UnityEngine.UI;

namespace GameScene.HUD
{
    public partial class HUDController : MonoBehaviour
    {
        [SerializeField] protected Slider healthSlider;
        [SerializeField] protected Text healthCounter;
        [SerializeField] protected Slider fuelSlider;
        [SerializeField] protected Text fuelCounter;
        [SerializeField] protected Slider dashCooldown;
        [SerializeField] protected Text ammoCounter;
        [SerializeField] protected GameObject hitMarker;
        [SerializeField] protected Image weaponPlaceHolder;
        [SerializeField] protected Text currentStrategy;
        [SerializeField] private GameObject itemSelector;
        [SerializeField] protected GameObject itemWheel;
        [SerializeField] protected Text MoneyLevel;
        [SerializeField] private MinionWheelController minionWheel;
        [SerializeField] private ShopUI shopUI;

        public float CanvasWidth => GetComponent<RectTransform>().rect.width;
        public float CanvasHeight => GetComponent<RectTransform>().rect.height;

        public ShopUI ShopUI => shopUI;


        public Image Crosshair;

        public static HUDController Singleton { get; private set; }

        private void Awake()
        {
            if (Singleton != null && Singleton != this)
            {
                Destroy(this);
                return;
            }

            Singleton = this;
        }

        void Start()
        {
            SetMaxHealth(100);
            SetMaxFuel(100);

            ShowHitMarker(false);
            minionWheel.IsActive = false;
            BaseWeapon.targetHit += ShowHitMarker;
            ObjectiveController.OnPlayerInteract += DisplayCaptureState;

            capturingState.enabled = false;
            shopUI.gameObject.SetActive(false);

            BasePlayer localPlayer = GameController.Singleton.LocalPlayer;

            arrow.transform.SetParent(map.transform);

            SetupSprites();

            _mapLoc = pointParent.transform.localPosition;
        }

        void Update()
        {
            var localPlayer = GameController.Singleton.LocalPlayer;
            if (localPlayer == null)
                return;

            if (localPlayer.Inventory.CurrentWeapon != null)
            {
                SetAmmoCounter(localPlayer.Inventory.CurrentWeapon.CurrentAmmo,
                    localPlayer.Inventory.CurrentWeapon.MaxAmmo);
                weaponPlaceHolder.sprite = localPlayer.Inventory.CurrentWeapon.Sprite;
            }

            SetHealth(localPlayer.CurrentHealth);
            SetFuelAmount(localPlayer.Jetpack.FuelConsumption);
            SetDashCooldown(localPlayer.DashedSince,
                localPlayer.DashCooldown);
            SetCurrentStrategy(localPlayer.Strategy);
            SetMoney(localPlayer.Money);
            SetRemainingItems();


            itemWheel.SetActive(localPlayer.ItemWheel);

            // updating the capture circle UI if the player is on a point
            if (_capturePoint != null)
            {
                capturingState.fillAmount = _capturePoint.CurrentProgress / _capturePoint.MaxProgress;
                SetCapIconColor();
            }

            UpdateMap();
        }

        /**
         * <summary> Sets the player's health </summary>
         * <param name="health">int for the max health</param>
         */
        public void SetMaxHealth(int health)
        {
            healthSlider.maxValue = health;

            healthSlider.value = health;
            healthCounter.text = $"{health}";
        }

        public void SetMaxFuel(int fuel)
        {
            fuelSlider.maxValue = fuel;

            fuelSlider.value = fuel;
            fuelCounter.text = $"{fuel}";
        }

        /**
         * <summary>sets the player's current health </summary>
         * <param name="health">int for the current health</param>
         */
        public void SetHealth(int health)
        {
            healthSlider.maxValue = GameController.Singleton.LocalPlayer.MaxHealth;
            healthSlider.value = health;
            healthCounter.text = $"{health}";
        }

        /**
         * <summary>sets the player's current fuel level </summary>
         * <param name="fuel">float for the current fuel level</param>
         */
        public void SetFuelAmount(float fuel)
        {
            fuelSlider.value = fuel * 100;
            fuelCounter.text = $"{Mathf.Floor(fuel * 100)}";
        }

        /**
         * <summary>sets the player's current dash cooldown </summary>
         * <param name="dashCd">float for the time since last dash</param>
         * <param name="maxDashCd">float for the base dash cooldown</param>
         */
        public void SetDashCooldown(float dashCd, float maxDashCd)
        {
            float value;
            if (dashCd == 0)
                value = 0;
            else
                value = 1 - dashCd / maxDashCd;
            dashCooldown.value = value;
        }

        /**
         * <summary>sets the player's current ammo count</summary>
         * <param name="ammo">int for the number of ammunition remaining</param>
         * <param name="maxAmmo">int for the max number of ammunition</param>
         */
        public void SetAmmoCounter(int ammo, int maxAmmo)
        {
            ammoCounter.text = $"{ammo}/{maxAmmo}";
        }

        /**
         * <summary>sets the next minion's strategy</summary>
         * <param name="strat">current minion's strategy</param>
         */
        public void SetCurrentStrategy(IMinion.Strategy strat)
        {
            string str = "Next minion: ";
            switch (strat)
            {
                case IMinion.Strategy.ATTACK:
                    str += "Attack";
                    break;
                case IMinion.Strategy.DEFEND:
                    str += "Defend";
                    break;
                case IMinion.Strategy.PROTECT:
                    str += "Protect";
                    break;
            }

            currentStrategy.text = str;
        }

        public void SetRemainingItems()
        {
            ItemWheel wheel = gameObject.AddComponent<ItemWheel>();
            Text[] ammos = itemWheel.GetComponentsInChildren<Text>();
            for (int i = 0; i < ammos.Length; i++)
                if (wheel.items[i] != null)
                {
                    try
                    {
                        ammos[i].text = GameController.Singleton.LocalPlayer.Inventory.ItemRegistry[wheel.items[i]]
                            .Count.ToString();
                    }
                    catch (KeyNotFoundException)
                    {
                        ammos[i].text = "0";
                    }
                }
        }

        /**
         * <summary>displays and hides the hit marker on the screen</summary>
         * <param name="status">bool for whether the hit marker should be shown or not</param>
         */
        public void ShowHitMarker(bool status)
        {
            hitMarker.SetActive(status);
        }

        public void ShowItemSelector(Vector2 screenPos, string itemName = null)
        {
            Vector2 positions = new Vector2(CanvasWidth * screenPos.x, CanvasHeight * screenPos.y);
            itemSelector.GetComponent<RectTransform>().anchoredPosition = positions;
            itemSelector.SetActive(true);
            itemSelector.GetComponentInChildren<Text>().text = itemName;
        }

        public void ShowMinionSelection()
        {
            minionWheel.IsActive = true;
        }

        public IMinion.Strategy HideMinionSelection()
        {
            minionWheel.IsActive = false;
            return minionWheel.strategy;
        }

        public void HideItemSelector()
        {
            itemSelector.SetActive(false);
        }

        private void SetupSprites()
        {
            ItemWheel wheel = gameObject.AddComponent<ItemWheel>();
            Image[] sprites = itemWheel.GetComponentsInChildren<Image>();
            for (int i = 1; i < sprites.Length; i++)
                if (wheel.items[i - 1] != null)
                    sprites[i].sprite = BaseItem.ItemInfos[wheel.items[i - 1]].Sprite;
        }

        private void SetMoney(int money)
        {
            MoneyLevel.text = "Player's Money: " + money;
        }
    }
}