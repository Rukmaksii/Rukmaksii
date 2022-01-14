using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HUDController : MonoBehaviour
{

    public Slider slider;

    public Text healthCounter;

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
}
