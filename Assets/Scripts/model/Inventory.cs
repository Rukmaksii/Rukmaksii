using System.Collections.Generic;
using Items;
using PlayerControllers;
using Unity.Netcode;
using UnityEngine;
using Weapons;

namespace model
{
    public class Inventory : NetworkBehaviour
    {
        private NetworkBehaviourReference playerReference = new NetworkBehaviourReference();

        /**
         * <value>the bound player <seealso cref="BasePlayer"/></value>
         */
        public BasePlayer Player
        {
            set => playerReference = new NetworkBehaviourReference(value);
            get => playerReference.TryGet<BasePlayer>(out BasePlayer p) ? p : null;
        }

        /**
         * <value>the <see cref="Jetpack"/> bound to the <see cref="Player"/></value>
         */
        private Jetpack _jetpack;

        public List<GameObject> Weapons
        {
            get
            {
                List<GameObject> res = new List<GameObject>();
                if (closeRangeWeapon != null)
                    res.Add(closeRangeWeapon.gameObject);
                if (lightWeapon != null)
                    res.Add(lightWeapon.gameObject);
                if (heavyWeapon != null)
                    res.Add(heavyWeapon.gameObject);


                return res;
            }
        }

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

                    availableWeapon = closeRangeWeapon;
                }

                if (lightWeapon != null)
                {
                    if (selectedType == lightWeapon.Type)
                    {
                        return lightWeapon;
                    }

                    availableWeapon = lightWeapon;
                }

                if (heavyWeapon != null)
                {
                    if (selectedType == heavyWeapon.Type)
                    {
                        return heavyWeapon;
                    }

                    availableWeapon = heavyWeapon;
                }

                return availableWeapon;
            }
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
                        Destroy(heavyWeapon.gameObject);
                    heavyWeapon = newWeapon;
                    break;
                case WeaponType.Light:
                    if (lightWeapon != null)
                        Destroy(lightWeapon.gameObject);
                    lightWeapon = newWeapon;
                    break;
                case WeaponType.CloseRange:
                    if (closeRangeWeapon)
                        Destroy(closeRangeWeapon);
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
                        selectedType = WeaponType.Heavy;
                        switched = true;
                    }

                    break;
                case WeaponType.Light:
                    if (lightWeapon != null)
                    {
                        selectedType = WeaponType.Light;
                        switched = true;
                    }

                    break;
                case WeaponType.CloseRange:
                    if (closeRangeWeapon != null)
                    {
                        selectedType = WeaponType.CloseRange;
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
            for (int i = offset + 1; i < 3 && !switched; i++)
            {
                WeaponType t = (WeaponType) i;
                switched = SwitchWeapon(t);
            }

            return switched;
        }

        public bool PreviousWeapon()
        {
            int offset = (int) CurrentWeapon.Type;
            bool switched = false;
            for (int i = offset - 1; i > 0 && !switched; i--)
            {
                WeaponType t = (WeaponType) i;
                switched = SwitchWeapon(t);
            }

            return switched;
        }

        private List<BaseItem> itemsList = new List<BaseItem>();

        public List<BaseItem> ItemsList => itemsList;

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
                    Destroy(element.gameObject);
                    break;
                }
            }
        }
    }
}