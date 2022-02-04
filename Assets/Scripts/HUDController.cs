using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GameManagers;
using PlayerControllers;
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
    
    [SerializeField] protected Sprite rifle;
    [SerializeField] protected Sprite handgun;

    private ObjectiveController capturePoint;
    
    private GameController gameController;
    
    public Image Crosshair;

    void Start()
    {
        GameObject gameManager = GameObject.FindGameObjectWithTag("GameController");
        gameController = gameManager.GetComponent<GameController>();
        
        SetMaxHealth(100);
        SetMaxFuel(100);

        ShowHitMarker(false);
        BaseWeapon.playerShot += ShowHitMarker;
        ObjectiveController.OnPlayerInteract += UpdateCaptureState;
        
        capturingState.enabled = false;
    }
    void Update()
    {
        if (gameController.LocalPlayer == null)
            return;

        // display the correct weapon on the HUD
        switch (gameController.LocalPlayer.Inventory.CurrentWeapon.Name)
        {
            case "rifle":
                weaponPlaceHolder.sprite = rifle;
                break;
            case "handgun":
                weaponPlaceHolder.sprite = handgun;
                break;
            default:
                weaponPlaceHolder.sprite = handgun;
                break;
        }

        SetHealth(gameController.LocalPlayer.CurrentHealthValue);
        SetFuelAmount(gameController.LocalPlayer.Jetpack.FuelConsumption);
        SetDashCooldown(gameController.LocalPlayer.DashedSince, gameController.LocalPlayer.DashCooldown);
        SetAmmoCounter(gameController.LocalPlayer.Inventory.CurrentWeapon.CurrentAmmo,gameController.LocalPlayer.Inventory.CurrentWeapon.MaxAmmo);

        if (capturePoint != null)
        {
            capturingState.fillAmount = capturePoint.Progress/4;
        }
    }
    
    /**
     * <summary>
     *      Sets the player's health
     * </summary>
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
        healthSlider.value = health;
        healthCounter.text = $"{health}";
    }
    
    /**
     * <summary>sets the player's current fuel level </summary>
     * <param name="fuel">float for the current fuel level</param>
     */
    public void SetFuelAmount(float fuel)
    {
        fuelSlider.value = fuel*100;
        fuelCounter.text = $"{Mathf.Floor(fuel*100)}";
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

    public void UpdateCaptureState(ObjectiveController area,BasePlayer player, bool state)
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
