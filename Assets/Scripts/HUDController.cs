using GameManagers;
using Map;
using model;
using PlayerControllers;
using UnityEngine;
using UnityEngine.UI;
using Weapons;

public class HUDController : MonoBehaviour
{
    [SerializeField] protected Slider healthSlider;
    [SerializeField] protected Text healthCounter;

    [SerializeField] protected Slider fuelSlider;
    [SerializeField] protected Text fuelCounter;

    [SerializeField] protected Slider dashCooldown;

    [SerializeField] protected Text ammoCounter;

    [SerializeField] protected GameObject hitMarker;

    [SerializeField] protected Image weaponPlaceHolder;

    [SerializeField] protected Image capturingState;
    
    [SerializeField] protected Text currentStrategy;

    private ObjectiveController capturePoint;

    private BasePlayer localPlayer;

    public Image Crosshair;

    void Start()
    {

        SetMaxHealth(100);
        SetMaxFuel(100);

        ShowHitMarker(false);
        BaseWeapon.targetHit += ShowHitMarker;
        ObjectiveController.OnPlayerInteract += DisplayCaptureState;

        capturingState.enabled = false;

        BasePlayer localPlayer = GameController.Singleton.LocalPlayer;
    }

    void Update()
    {
        if (GameController.Singleton.LocalPlayer == null)
            return;

        weaponPlaceHolder.sprite = GameController.Singleton.LocalPlayer.Inventory.CurrentWeapon.sprite;

        SetHealth(GameController.Singleton.LocalPlayer.CurrentHealthValue);
        SetFuelAmount(GameController.Singleton.LocalPlayer.Jetpack.FuelConsumption);
        SetDashCooldown(GameController.Singleton.LocalPlayer.DashedSince, GameController.Singleton.LocalPlayer.DashCooldown);
        SetAmmoCounter(GameController.Singleton.LocalPlayer.Inventory.CurrentWeapon.CurrentAmmo,
            GameController.Singleton.LocalPlayer.Inventory.CurrentWeapon.MaxAmmo);
        SetCurrentStrategy((IMinion.Strategy) GameController.Singleton.LocalPlayer.Strategy);

        // updating the capture circle UI if the player is on a point
        if (capturePoint != null)
        {
            capturingState.fillAmount = capturePoint.CurrentProgress / capturePoint.MaxProgress;
            SetCapIconColor();
        }
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
    
    /**
     * <summary>displays and hides the hit marker on the screen</summary>
     * <param name="status">bool for whether the hit marker should be shown or not</param>
     */
    public void ShowHitMarker(bool status)
    {
        if (!status)
        {
            hitMarker.SetActive(false);
        }
        else
        {
            hitMarker.SetActive(true);
        }
    }

    /**
     * <summary>displays and hides the circle for capturing an objective</summary>
     * <param name="area">ObjectiveController for the objective being captured</param>
     * <param name="player">BasePlayer for the player capturing it</param>
     * <param name="state">bool for whether the player enters or leaves the objective</param>
     */
    public void DisplayCaptureState(ObjectiveController area, BasePlayer player, bool state)
    {
        if (player == GameController.Singleton.LocalPlayer)
        {
            if (state)
            {
                capturePoint = area;
                capturingState.enabled = true;
            }
            else
            {
                capturePoint = null;
                capturingState.enabled = false;
            }
        }
    }

    private void SetCapIconColor()
    {
        if (capturePoint.CapturingTeam == 1)
            capturingState.color = Color.red;
        else if (capturePoint.CapturingTeam == 0)
            capturingState.color = Color.blue;
    }
}