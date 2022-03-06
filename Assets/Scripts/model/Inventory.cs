using System.Collections.Generic;
using Items;
using PlayerControllers;
using UnityEngine;
using Weapons;

namespace model
{
    public class Inventory
    {
        /**
         * <value>the bound player <seealso cref="BasePlayer"/></value>
         */
        private readonly BasePlayer Player;

        /**
         * <value>the <see cref="Jetpack"/> bound to the <see cref="Player"/></value>
         */
        private Jetpack _jetpack;

        public Jetpack Jetpack
        {
            get => _jetpack;
            set
            {
                _jetpack = value;
                _jetpack.Player = Player;
            }
        }

        /**
         * <value>the close range weapon</value>
         */
        private BaseWeapon closeRangeWeapon;

        /**
         * <value>the heavy weapon</value>
         */
        private BaseWeapon heavyWeapon;

        /**
         * <value>the light weapon</value>
         */
        private BaseWeapon lightWeapon;

        public BaseWeapon HeavyWeapon => heavyWeapon;
        public BaseWeapon CloseRangeWeapon => closeRangeWeapon;
        public BaseWeapon LightWeapon => lightWeapon;

        /**
         * <value>the <see cref="WeaponType"/> of the currently selected weapon</value>
         * <remarks>set to <see cref="WeaponType.CloseRange"/> as it is assumed the close range weapon will never be null</remarks>
         */
        private WeaponType selectedType = WeaponType.CloseRange;

        /**
         * <value>the currently selected weapon</value>
         * <remarks>should not be null as at least <see cref="closeRangeWeapon"/> should not be null</remarks>
         */
        public BaseWeapon CurrentWeapon
        {
            get
            {
                BaseWeapon availableWeapon = null;
                if (closeRangeWeapon != null)
                {
                    if (selectedType == closeRangeWeapon.Type)
                    {
                        return closeRangeWeapon;
                    }
                    else
                    {
                        availableWeapon = closeRangeWeapon;
                    }
                }
                else if (lightWeapon != null)
                {
                    if (selectedType == lightWeapon.Type)
                    {
                        return lightWeapon;
                    }
                    else
                    {
                        availableWeapon = lightWeapon;
                    }
                }
                else if (heavyWeapon != null)
                {
                    if (selectedType == heavyWeapon.Type)
                    {
                        return heavyWeapon;
                    }
                    else
                    {
                        availableWeapon = heavyWeapon;
                    }
                }

                return availableWeapon;
            }
        }

        public Inventory(BasePlayer player)
        {
            this.Player = player;
        }


        /**
         * <summary>adds a weapon to the inventory replacing the old weapon of the same <see cref="WeaponType"/> if existing</summary>
         */
        public void AddWeapon(BaseWeapon newWeapon)
        {
            newWeapon.Player = Player;
            switch (newWeapon.Type)
            {
                case WeaponType.Heavy:
                    if (heavyWeapon != null)
                        Object.Destroy(heavyWeapon.gameObject);
                    heavyWeapon = newWeapon;
                    break;
                case WeaponType.Light:
                    if (lightWeapon != null)
                        Object.Destroy(lightWeapon.gameObject);
                    lightWeapon = newWeapon;
                    break;
                case WeaponType.CloseRange:
                    if (closeRangeWeapon)
                        Object.Destroy(closeRangeWeapon);
                    closeRangeWeapon = newWeapon;
                    break;
            }
        }

        /**
         * <summary>changes the currently selected weapon if existing</summary>
         * <returns>false if the provided <see cref="WeaponType"/> not be found</returns>
         */
        public bool SwitchWeapon(WeaponType type)
        {
            bool switched = false;

            switch (type)
            {
                case WeaponType.Heavy:
                    if (heavyWeapon != null)
                    {
                        selectedType = type;
                        switched = true;
                    }

                    break;
                case WeaponType.Light:
                    if (lightWeapon != null)
                    {
                        selectedType = type;
                        switched = true;
                    }

                    break;
                case WeaponType.CloseRange:
                    if (closeRangeWeapon != null)
                    {
                        selectedType = type;
                        switched = true;
                    }

                    break;
            }

            return switched;
        }

        public bool NextWeapon()
        {
            int offset = (int) CurrentWeapon.Type;
            bool switched = false;
            for (int i = 1; i < 3 && !switched; i++)
            {
                WeaponType t = (WeaponType) ((offset + i) % 3);
                switched = SwitchWeapon(t);
            }


            Debug.Log($"current weapon: {CurrentWeapon.Type}");
            Debug.Log($"heavy: {this.heavyWeapon != null}, light: {this.lightWeapon != null}");
            return switched;
        }

        public bool PreviousWeapon()
        {
            int offset = (int) CurrentWeapon.Type;
            bool switched = false;
            for (int i = 2; i > 0 && !switched; i--)
            {
                WeaponType t = (WeaponType) ((offset + i) % 3);
                switched = SwitchWeapon(t);
            }

            return switched;
        }

        private List<BaseItem> itemsList = new List<BaseItem>();

        /**
         * <summary>adds an instantiated item to the inventory</summary>
         * <param name="item">a BaseItem to be added</param>
         */
        public void AddItem(BaseItem item)
        {
            item.Player = Player;
            if (!itemsList.Contains(item))
            {
                itemsList.Add(item);
            }
        }

        /**
         * <summary>if the item is in the inventory, it removes it
         * also it destroys the item from the scene</summary>
         * <param name="item">a BaseItem to be removed</param>
         */
        public void RemoveItem(BaseItem item)
        {
            foreach (BaseItem element in itemsList)
            {
                if (item.Type == element.Type)
                {
                    itemsList.Remove(element);
                    Object.Destroy(element.gameObject);
                    break;
                }
            }
        }
    }
}