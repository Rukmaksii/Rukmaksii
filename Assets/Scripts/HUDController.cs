using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HUDController : MonoBehaviour
{

    [SerializeField] protected Slider slider;

    [SerializeField] protected Text healthCounter;

    private GameController gameController;
    
    public Image Crosshair;

    private PlayerController controller;

    public void SetController(PlayerController _controller)
    {
        controller = _controller;
    }

    void Start()
    {
        GameObject gameManager = GameObject.FindGameObjectWithTag("GameController");
        gameController = gameManager.GetComponent<GameController>();
    }
    void Update()
    {
        if (gameController.LocalPlayer == null)
            return;
        SetHealth(gameController.LocalPlayer.GetCurrentHealth());
    }
    
    /**
     * <summary>
     *      Sets the player's health
     * </summary>
     * <param name="health">int for the max health</param>
     */
    public void SetMaxHealth(int health)
    {
        slider.maxValue = health;

        slider.value = health;
        healthCounter.text = "" + health;
    }

    /**
     * <summary>
     *      Sets the player's current health
     * </summary>
     * <param name="health">int for the current health</param>
     */
    public void SetHealth(int health)
    {
        slider.value = health;
        healthCounter.text = "" + health;
    }
    
    public void SetFuelAmount(int fuel)
    {
        slider.value = fuel;
        healthCounter.text = "" + fuel;
    }
}
