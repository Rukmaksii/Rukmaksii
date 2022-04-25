using System;
using System.Collections;
using System.Collections.Generic;
using Items;
using JetBrains.Annotations;
using model;
using PlayerControllers;
using UnityEditor;
using UnityEngine;

public class ItemWheel : MonoBehaviour
{
    public Type[] items = {typeof(FuelBooster), typeof(ItemTest), null, null, null, null, null, null};

    public void SelectItem(Vector3 mousePosition, BasePlayer Player)
    {
        float x = Input.mousePosition.x - mousePosition.x;
        float y = Input.mousePosition.y - mousePosition.y;

        if (x > 0)
        {
            if (y > 0)
            {
                if (y > x)
                    Select(0, Player);
                else
                    Select(1, Player);
            }
            else
            {
                if (Math.Abs(y) > x)
                    Select(3, Player);
                else
                    Select(2, Player);
            }
        }
        else
        {
            if (y > 0)
            {
                if (y > Math.Abs(x))
                    Select(7, Player);
                else
                    Select(6, Player);
            }
            else
            {
                if (Math.Abs(y) > Math.Abs(x))
                    Select(4, Player);
                else
                    Select(5, Player);
            }
        }
    }

    private void Select(int i, BasePlayer Player)
    {
        if (Player.Inventory.SelectedItem != null && items[i] != null)
        {
            Player.Inventory.SelectedItem.SwitchRender(false);
            Player.Inventory.SelectedItemType = items[i];
        }
    }
}