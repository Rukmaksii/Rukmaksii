using System;
using System.Collections.Generic;
using System.Linq;
using Items;
using model.Network;
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

                if (availableWeapon != null)
                    SelectedType = availableWeapon.Type;
                return availableWeapon;
            }
        }

        private NetworkList<NetworkBehaviourReference> itemsList;
        private NetworkItemRegistry itemRegistry;

        private void Awake()
        {
            itemsList = new NetworkList<NetworkBehaviourReference>();
            itemRegistry = new NetworkItemRegistry();
        }

        /**
         * <summary>adds a weapon to the inventory replacing the old weapon of the same <see cref="WeaponType"/> if existing</summary>
         */
        public void AddWeapon(BaseWeapon newWeapon)
        {
            var weaponRef = new NetworkBehaviourReference(newWeapon);
            AddWeaponServerRpc(weaponRef, newWeapon.Type);
        }

        /// <summary>
        ///     drops the current weapon 
        /// </summary>
        /// <returns>true if the weapon was dropped</returns>
        public bool DropCurrentWeapon()
        {
            if (Weapons.Count <= 1)
                return false;

            switch (SelectedType)
            {
                case WeaponType.Heavy:
                    DropWeaponServerRpc(heavyWeapon.Value);
                    break;
                case WeaponType.Light:
                    DropWeaponServerRpc(lightWeapon.Value);
                    break;
                case WeaponType.CloseRange:
                    DropWeaponServerRpc(closeRangeWeapon.Value);
                    break;
            }

            return true;
        }

        /**
         * <summary>changes the currently selected weapon if existing</summary>
         * <returns>false if the provided <see cref="WeaponType"/> not be found</returns>
         */
        public bool SwitchWeapon(WeaponType type)
        {
            if (type == CurrentWeapon.Type || Weapons.Count <= 1)
                return false;
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

        private Dictionary<Type, ItemContainer<BaseItem>> itemsDictionary =
            new Dictionary<Type, ItemContainer<BaseItem>>();

        /**
         * <summary>adds an instantiated item to the inventory</summary>
         * <param name="item">a BaseItem to be added</param>
         */
        public void AddItem(BaseItem item)
        {
            if (item.State != ItemState.Clean)
                return;
            itemRegistry[item.GetType()].Push(item);
            AddItemServerRpc(new NetworkBehaviourReference(item));
        }

        [ServerRpc(RequireOwnership = false)]
        private void AddItemServerRpc(NetworkBehaviourReference itemRef)
        {
            this.itemsList.Add(itemRef);
            itemRef.TryGet(out BaseItem item);
            item.PickUp(Player);
        }

        public NetworkItemRegistry.ItemContainer GetItemContainer<TForMethod>() where TForMethod : BaseItem
        {
            return itemRegistry[typeof(TForMethod)];
        }

        [ServerRpc]
        private void RemoveItemsServerRpc(NetworkBehaviourReference[] itemRefs)
        {
            for (int i = 0; i < itemsList.Count; i++)
            {
                itemsList[i].TryGet(out BaseItem item);
                if (item.State == ItemState.Consumed || itemRefs.Contains(itemsList[i]))
                    itemsList.RemoveAt(i--);
            }
        }

        /**
         * <summary>if the item is in the inventory, it removes it
         * also it destroys the item from the scene</summary>
         * <param name="item">a BaseItem to be removed</param>
         */
        public void RemoveItem(BaseItem item)
        {
            BaseItem element = itemsDictionary[item.GetType()].Pop();
            if (element != null)
                Destroy(element.gameObject);
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
                    if (heavyWeapon.Value.TryGet(out BaseWeapon oldWeapon))
                    {
                        oldWeapon.Drop();
                        DropWeaponClientRpc(heavyWeapon.Value);
                    }

                    heavyWeapon.Value = weaponRef;
                    break;
                case WeaponType.Light:
                    if (lightWeapon.Value.TryGet(out oldWeapon))
                    {
                        oldWeapon.Drop();
                        DropWeaponClientRpc(lightWeapon.Value);
                    }

                    lightWeapon.Value = weaponRef;
                    break;
                case WeaponType.CloseRange:
                    if (closeRangeWeapon.Value.TryGet(out oldWeapon))
                    {
                        oldWeapon.Drop();
                        DropWeaponClientRpc(closeRangeWeapon.Value);
                    }

                    closeRangeWeapon.Value = weaponRef;
                    break;
            }


            weaponRef.TryGet(out BaseWeapon weapon);
            weapon.PickUp(Player);
            SwitchWeaponServerRpc(type);
        }

        [ServerRpc]
        private void DropWeaponServerRpc(NetworkBehaviourReference weaponRef)
        {
            if (!weaponRef.TryGet(out BaseWeapon weapon))
                return;

            weapon.Drop();
            switch (weapon.Type)
            {
                case WeaponType.Heavy:
                    heavyWeapon.Value = new NetworkBehaviourReference();
                    break;
                case WeaponType.Light:
                    lightWeapon.Value = new NetworkBehaviourReference();
                    break;
                case WeaponType.CloseRange:
                    closeRangeWeapon.Value = new NetworkBehaviourReference();
                    break;
            }

            SelectedType = Weapons.Select(v => v.GetComponent<BaseWeapon>()).First().Type;
            DropWeaponClientRpc(weaponRef);
        }

        [ClientRpc]
        private void DropWeaponClientRpc(NetworkBehaviourReference oldWeaponRef)
        {
            oldWeaponRef.TryGet(out BaseWeapon oldWeapon);
            oldWeapon.SwitchRender(true);
        }


        private BaseWeapon GetWeaponByType(WeaponType type)
        {
            return type switch
            {
                WeaponType.Light => LightWeapon,
                WeaponType.Heavy => HeavyWeapon,
                _ => CloseRangeWeapon
            };
        }

        [ServerRpc]
        private void SwitchWeaponServerRpc(WeaponType type)
        {
            if (SelectedType != type)
            {
                SwitchWeaponClientRpc(SelectedType, type);
                selectedType.Value = type;
            }
        }

        [ClientRpc]
        private void SwitchWeaponClientRpc(WeaponType oldType, WeaponType type)
        {
            BaseWeapon baseWeapon = GetWeaponByType(type);
            BaseWeapon oldWeapon = GetWeaponByType(oldType);

            if (oldWeapon != null && oldWeapon.IsOwned)
                oldWeapon.SwitchRender(false);
            if (baseWeapon != null && baseWeapon.IsOwned)
            {
                Player.SetHandTargets(baseWeapon.RightHandTarget, baseWeapon.LeftHandTarget);
                baseWeapon.SwitchRender(true);
            }
        }
    }
}