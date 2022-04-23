using System;
using System.Collections;
using System.Collections.Generic;
using Items;
using PlayerControllers;
using UnityEditor;
using UnityEngine;

public class ItemWheel : MonoBehaviour
{
    private BaseItem item1;
    private BaseItem item2;
    private BaseItem item3;
    private BaseItem item4;
    private BaseItem item5;
    private BaseItem item6;
    private BaseItem item7;
    private BaseItem item8;
    
    public void SelectItem(Vector3 mousePosition, BasePlayer Player)
    {
        float x = Input.mousePosition.x - mousePosition.x;
        float y = Input.mousePosition.y - mousePosition.y;
        
        if (x > 0)
        {
            if (y > 0)
            {
                if (y > x)
                {
                    Debug.Log("item1");
                }
                else
                {
                    Debug.Log("item2");
                }
            }
            else
            {
                if (Math.Abs(y) > x)
                {
                    Debug.Log("item4");
                }
                else
                {
                    Debug.Log("item3");
                }
            }
        }
        else
        {
            if (y > 0)
            {
                if (y > Math.Abs(x))
                {
                    Debug.Log("item8");
                }
                else
                {
                    Debug.Log("item7");
                }
            }
            else
            {
                if (Math.Abs(y) > Math.Abs(x))
                {
                    Debug.Log("item5");
                }
                else
                {
                    Debug.Log("item6");
                }
            }
        }
    }
}
