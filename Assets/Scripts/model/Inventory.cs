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
        private NetworkVariable<NetworkBehaviourReference> playerReference =
            new NetworkVariable<NetworkBehaviourReference>();

        /**
         * <value>the bound player <seealso cref="BasePlayer"/></value>
         */
        public BasePlayer Player
        {
            set => UpdatePlayerReferenceServerRpc(new NetworkBehaviourReference(value));
            get => playerReference.Value.TryGet<BasePlayer>(out BasePlayer p) ? p : null;
        }

        public List<GameObject> Weapons
        {
            get
            {
                List<GameObject> res = new List<GameObject>();
                if (CloseRangeWeapon != null)
                    res.Add(CloseRangeWeapon.gameObject);
                if (LightWeapon != null)
                    res.Add(LightWeapon.gameObject);
                if (HeavyWeapon != null)
                    res.Add(HeavyWeapon.gameObject);


                return res;
            }
        }

        /**
         * <value>the close range weapon</value>
         */
        private NetworkVariable<NetworkBehaviourReference> closeRangeWeapon =
            new NetworkVariable<NetworkBehaviourReference>();

        /**
         * <value>the heavy weapon</value>
         */
        private NetworkVariable<NetworkBehaviourReference> heavyWeapon =
            new NetworkVariable<NetworkBehaviourReference>();

        /**
         * <value>the light weapon</value>
         */
        private NetworkVariable<NetworkBehaviourReference> lightWeapon =
            new NetworkVariable<NetworkBehaviourReference>();

        public BaseWeapon HeavyWeapon => heavyWeapon.Value.TryGet<BaseWeapon>(out BaseWeapon res) ? res : null;

        public BaseWeapon CloseRangeWeapon =>
            closeRangeWeapon.Value.TryGet<BaseWeapon>(out BaseWeapon res) ? res : null;

        public BaseWeapon LightWeapon => lightWeapon.Value.TryGet<BaseWeapon>(out BaseWeapon res) ? res : null;

        /**
         * <value>the <see cref="WeaponType"/> of the currently selected weapon</value>
         * <remarks>set to <see cref="WeaponType.CloseRange"/> as it is assumed the close range weapon will never be null</remarks>
         */
        private NetworkVariable<WeaponType> selectedType = new NetworkVariable<WeaponType>(WeaponType.CloseRange);

        private WeaponType SelectedType
        {
            get => selectedType.Value;
            set => SwitchWeaponServerRpc(value);
        }

        /**
         * <value>the currently selected weapon</value>
         * <remarks>should not be null as at least <see cref="closeRangeWeapon"/> should not be null</remarks>
         */
        public BaseWeapon CurrentWeapon
        {
            get
            {
                BaseWeapon availableWeapon = null;
                if (CloseRangeWeapon != null)
                {
                    if (SelectedType == CloseRangeWeapon.Type)
                    {
                        return CloseRangeWeapon;
                    }

                    availableWeapon = CloseRangeWeapon;
                }

                if (LightWeapon != null)
                {
                    if (SelectedType == LightWeapon.Type)
                    {
                        return LightWeapon;
                    }

                    availableWeapon = LightWeapon;
                }

                if (HeavyWeapon != null)
                {
                    if (SelectedType == HeavyWeapon.Type)
                    {
                        return HeavyWeapon;
                    }

                    availableWeapon = HeavyWeapon;
                }

                Debug.Log($"available weapon: {availableWeapon}");
                return availableWeapon;
            }
        }

        /**
         * <summary>adds a weapon to the inventory replacing the old weapon of the same <see cref="WeaponType"/> if existing</summary>
         */
        public void AddWeapon(BaseWeapon newWeapon)
        {
            newWeapon.Player = Player;
            var weaponRef = new NetworkBehaviourReference(newWeapon);
            Debug.Log($"adding {newWeapon.Name}");
            AddWeaponServerRpc(weaponRef, newWeapon.Type);
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
                        SelectedType = WeaponType.Heavy;
                        switched = true;
                    }

                    break;
                case WeaponType.Light:
                    if (lightWeapon != null)
                    {
                        SelectedType = WeaponType.Light;
                        switched = true;
                    }

                    break;
                case WeaponType.CloseRange:
                    if (closeRangeWeapon != null)
                    {
                        SelectedType = WeaponType.CloseRange;
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

        [ServerRpc(RequireOwnership = false)]
        private void UpdatePlayerReferenceServerRpc(NetworkBehaviourReference playerRef)
        {
            this.playerReference.Value = playerRef;
        }

        [ServerRpc(RequireOwnership = false)]
        private void AddWeaponServerRpc(NetworkBehaviourReference weaponRef, WeaponType type)
        {
            switch (type)
            {
                case WeaponType.Heavy:
                    heavyWeapon.Value = weaponRef;
                    break;
                case WeaponType.Light:
                    lightWeapon.Value = weaponRef;
                    break;
                case WeaponType.CloseRange:
                    closeRangeWeapon.Value = weaponRef;
                    break;
            }
        }

        [ServerRpc]
        private void SwitchWeaponServerRpc(WeaponType type)
        {
            selectedType.Value = type;
        }
    }
}