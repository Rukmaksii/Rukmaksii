using System;
using System.Collections;
using System.Collections.Generic;
using Items;
using PlayerControllers;
using UnityEditor;
using UnityEngine;

public class ItemWheel : MonoBehaviour
{
    public BaseItem[] items = new BaseItem[8];

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
            Player.Inventory.SelectedItemType = items[i].GetType();
        }
    }

    public void AddItem(BaseItem item)
    {
        int i = 0;
        
        while (i < 8)
        {
            if (items[i] != null)
            {
                if (items[i].GetType() == item.GetType())
                    return;
            }
            else
            {
                items[i] = item;
                return;
            }
            i++;
        }
        
        Debug.Log("ItemWheel: Not enough space");
    }

    public void RemoveItem(BaseItem item)
    {
        int i = 0;
        
        while (i < 8 && items[i] != null)
        {
            if (items[i].GetType() == item.GetType())
            {
                items[i] = null;
                return;
            }
            i++;
        }
        
        Debug.Log("ItemWheel: Item not found");
    }
}