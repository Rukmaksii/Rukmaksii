using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GameManagers;

public class HUDController : MonoBehaviour
{

    [SerializeField] protected Slider healthSlider;
    [SerializeField] protected Text healthCounter;

    [SerializeField] protected Slider fuelSlider;
    [SerializeField] protected Text fuelCounter;
    
    private GameController gameController;
    
    public Image Crosshair;

    void Start()
    {
        GameObject gameManager = GameObject.FindGameObjectWithTag("GameController");
        gameController = gameManager.GetComponent<GameController>();
        
        SetMaxHealth(100);
        SetMaxFuel(100);
    }
    void Update()
    {
        if (gameController.LocalPlayer == null)
            return;
        SetHealth(gameController.LocalPlayer.GetCurrentHealth());
        SetFuelAmount(gameController.LocalPlayer.Jetpack.FuelConsumption);
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
        healthCounter.text = "" + health;
    }
    
    public void SetMaxFuel(int health)
    {
        fuelSlider.maxValue = health;

        fuelSlider.value = health;
        fuelCounter.text = "" + health;
    }

    /**
     * <summary>
     *      Sets the player's current health
     * </summary>
     * <param name="health">int for the current health</param>
     */
    public void SetHealth(int health)
    {
        healthSlider.value = health;
        healthCounter.text = "" + health;
    }
    
    public void SetFuelAmount(float fuel)
    {
        fuelSlider.value = fuel*100;
        fuelCounter.text = "" + Mathf.Floor(fuel*100);
    }
}
