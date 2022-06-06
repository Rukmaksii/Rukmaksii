using System;
using GameScene.PlayerControllers.BasePlayer;
using GameScene.PlayerControllers.Inventory;
using UnityEngine;

namespace GameScene.Items
{
    public class ItemWheel : MonoBehaviour
    {
        public Type[] items = {typeof(FuelBooster), typeof(Grenade), typeof(Bandage), typeof(Medkit), null, null, null, null};

        private bool isSwitchingItem;

        public bool IsSwitchingItem => isSwitchingItem;

        public void SelectItem(Vector3 mousePosition, BasePlayer Player)
        {
            isSwitchingItem = false;
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
            if (items[i] == null)
            {
                isSwitchingItem = false;
            }
            else
            {
                isSwitchingItem = items[i] != Player.Inventory.SelectedItemType || Player.Inventory.SelectedMode != Inventory.Mode.Item;
            }

            if (IsSwitchingItem)
            {
                Player.Inventory.SelectedItemType = items[i];
            }
        }
    }
}