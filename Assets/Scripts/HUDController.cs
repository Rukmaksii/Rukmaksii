using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HUDController : MonoBehaviour
{

    public Slider slider;

    public Text healthCounter;

    public void SetMaxHealth(int health)
    {
        slider.maxValue = health;
        
        slider.value = health;
        healthCounter.text = "" + health;
    }

    public void SetHealth(int health)
    {
        slider.value = health;
        healthCounter.text = "" + health;
    }
}
