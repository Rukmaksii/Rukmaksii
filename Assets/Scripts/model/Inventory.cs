using System.Collections.Generic;
using System.Linq;
using Items;
using PlayerControllers;
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
                    heavyWeapon = newWeapon;
                    break;
                case WeaponType.Light:
                    lightWeapon = newWeapon;
                    break;
                case WeaponType.CloseRange:
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

        private List<BaseItem> itemsList = new List<BaseItem>();

        /**
         * <summary>adds an item to the inventory if it isn't already in it
         * if the item has a passive, it activates it</summary>
         */
        public void AddItem(BaseItem newItem)
        {
            if (!itemsList.Contains(newItem))
            {
                itemsList.Add(newItem);

                switch (newItem.Name)
                {
                    case "FuelBooster" :
                        Player.Jetpack.FuelDuration = 1f;
                        break;
                    default:
                        break;
                }
            }
        }
        
        /**
         * <summary>adds an item to the inventory if it isn't already in it
         * if the item has a passive, it activates it</summary>
         */
        public void RemoveItem(BaseItem itemToRem)
        {
            if (itemsList.Contains(itemToRem))
            {
                itemsList.Remove(itemToRem);

                switch (itemToRem.Name)
                {
                    case "FuelBooster" :
                        itemToRem.RemovePassive();
                        break;
                    default:
                        break;
                }
            }
        }
    }
}