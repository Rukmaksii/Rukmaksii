using System.Collections.Generic;
using System.Linq;
using GameScene.model;
using GameScene.Weapons;
using Unity.Netcode;
using UnityEngine;

namespace GameScene.PlayerControllers.Inventory
{
    public partial class Inventory
    {
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
        private readonly NetworkVariable<NetworkBehaviourReference> closeRangeWeapon =
            new NetworkVariable<NetworkBehaviourReference>();

        /**
                 * <value>the heavy weapon</value>
                 */
        private readonly NetworkVariable<NetworkBehaviourReference> heavyWeapon =
            new NetworkVariable<NetworkBehaviourReference>();

        /**
                 * <value>the light weapon</value>
                 */
        private readonly NetworkVariable<NetworkBehaviourReference> lightWeapon =
            new NetworkVariable<NetworkBehaviourReference>();

        public BaseWeapon HeavyWeapon => heavyWeapon.Value.TryGet(out BaseWeapon res) ? res : null;

        public BaseWeapon CloseRangeWeapon =>
            closeRangeWeapon.Value.TryGet(out BaseWeapon res) ? res : null;

        public BaseWeapon LightWeapon => lightWeapon.Value.TryGet(out BaseWeapon res) ? res : null;

        /**
                 * <value>the <see cref="WeaponType"/> of the currently selected weapon</value>
                 * <remarks>set to <see cref="WeaponType.CloseRange"/> as it is assumed the close range weapon will never be null</remarks>
                 */
        private readonly NetworkVariable<WeaponType> selectedWeaponType =
            new NetworkVariable<WeaponType>(WeaponType.CloseRange);

        private WeaponType SelectedWeaponType
        {
            get => selectedWeaponType.Value;
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
                    if (SelectedWeaponType == CloseRangeWeapon.Type)
                    {
                        return CloseRangeWeapon;
                    }

                    availableWeapon = CloseRangeWeapon;
                }

                if (LightWeapon != null)
                {
                    if (SelectedWeaponType == LightWeapon.Type)
                    {
                        return LightWeapon;
                    }

                    availableWeapon = LightWeapon;
                }

                if (HeavyWeapon != null)
                {
                    if (SelectedWeaponType == HeavyWeapon.Type)
                    {
                        return HeavyWeapon;
                    }

                    availableWeapon = HeavyWeapon;
                }

                if (availableWeapon != null)
                    SelectedWeaponType = availableWeapon.Type;
                return availableWeapon;
            }
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
            if (SelectedMode != Mode.Weapon || Weapons.Count <= 1)
                return false;

            switch (SelectedWeaponType)
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
            SelectedMode = Mode.Weapon;
            if (type == CurrentWeapon.Type || Weapons.Count <= 1)
                return false;
            bool switched = false;

            switch (type)
            {
                case WeaponType.Heavy:
                    if (heavyWeapon != null)
                    {
                        SelectedWeaponType = WeaponType.Heavy;
                        switched = true;
                    }

                    break;
                case WeaponType.Light:
                    if (lightWeapon != null)
                    {
                        SelectedWeaponType = WeaponType.Light;
                        switched = true;
                    }

                    break;
                case WeaponType.CloseRange:
                    if (closeRangeWeapon != null)
                    {
                        SelectedWeaponType = WeaponType.CloseRange;
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

        [ServerRpc(RequireOwnership = false)]
        private void AddWeaponServerRpc(NetworkBehaviourReference weaponRef, WeaponType type)
        {
            switch (type)
            {
                case WeaponType.Heavy:
                    if (heavyWeapon.Value.TryGet(out BaseWeapon oldWeapon))
                    {
                        oldWeapon.Drop();
                        oldWeapon.SwitchRender(true);
                    }

                    heavyWeapon.Value = weaponRef;
                    break;
                case WeaponType.Light:
                    if (lightWeapon.Value.TryGet(out oldWeapon))
                    {
                        oldWeapon.Drop();
                        oldWeapon.SwitchRender(true);
                    }

                    lightWeapon.Value = weaponRef;
                    break;
                case WeaponType.CloseRange:
                    if (closeRangeWeapon.Value.TryGet(out oldWeapon))
                    {
                        oldWeapon.Drop();
                        oldWeapon.SwitchRender(true);
                    }

                    closeRangeWeapon.Value = weaponRef;
                    break;
            }


            weaponRef.TryGet(out BaseWeapon weapon);
            weapon.PickUp(Player);
            weapon.SwitchRender(true);
            SwitchWeaponServerRpc(type);
        }

        [ServerRpc]
        private void DropWeaponServerRpc(NetworkBehaviourReference weaponRef)
        {
            if (!weaponRef.TryGet(out BaseWeapon weapon))
                return;

            weapon.Drop();
            weapon.SwitchRender(true);
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

            SelectedWeaponType = Weapons.Select(v => v.GetComponent<BaseWeapon>()).First().Type;
            HandleModeRenderers(SelectedMode);
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

        [ServerRpc(RequireOwnership = false)]
        private void SwitchWeaponServerRpc(WeaponType type)
        {
            SelectedMode = Mode.Weapon;
            if (SelectedWeaponType != type)
            {
                var oldWeapon = GetWeaponByType(SelectedWeaponType);
                if (oldWeapon != null)
                    oldWeapon.SwitchRender(false);


                var weapon = GetWeaponByType(type);

                if (weapon != null)
                    weapon.SwitchRender(true);
                selectedWeaponType.Value = type;
            }

            SwitchWeaponClientRpc(type);
        }

        [ClientRpc]
        private void SwitchWeaponClientRpc(WeaponType type)
        {
            BaseWeapon baseWeapon = GetWeaponByType(type);
            if (baseWeapon != null && baseWeapon.IsOwned)
            {
                Player.SetHandTargets(baseWeapon.RightHandTarget, baseWeapon.LeftHandTarget);
            }
        }
    }
}