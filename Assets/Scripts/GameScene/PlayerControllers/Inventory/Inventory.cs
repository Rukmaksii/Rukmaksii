using GameScene.Abilities.model;
using GameScene.HUD;
using GameScene.Items;
using GameScene.PlayerControllers.BasePlayer;
using GameScene.Shop;
using GameScene.Weapons;
using Unity.Netcode;
using UnityEngine;

namespace GameScene.PlayerControllers.Inventory
{
    public partial class Inventory : NetworkBehaviour
    {
        /// <summary>
        ///     the mode of the inventory
        /// </summary>
        public enum Mode
        {
            Weapon,
            Item
        }


        private BasePlayer.BasePlayer _player;

        /**
         * <value>the bound player <seealso cref="BasePlayer"/></value>
         */
        public BasePlayer.BasePlayer Player
        {
            get
            {
                if (_player is null)
                {
                    _player = GetComponent<BasePlayer.BasePlayer>();
                    AbilityTree = new AbilityTree(_player);
                    if (HUDController.Singleton != null)
                    {
                        HUDController.Singleton.UpdateAbilities();
                    }
                }

                return _player;
            }
        }

        private readonly NetworkVariable<Mode> selectedMode = new NetworkVariable<Mode>();

        public Mode SelectedMode
        {
            get => selectedMode.Value;
            private set => UpdateModeServerRpc(value);
        }

        public AbilityTree AbilityTree { get; private set; }


        void Start()
        {
            if (ItemWheel == null)
                itemWheel = gameObject.AddComponent<ItemWheel>();

            if (IsServer)
                HandleModeRenderers(SelectedMode, true);

            void OnWeaponChanged(NetworkBehaviourReference old, NetworkBehaviourReference newVal)
            {
                if (newVal.TryGet(out BaseWeapon weapon))
                {
                    Player.SetHandTargets(weapon.RightHandTarget, weapon.LeftHandTarget);
                }
            }

            lightWeapon.OnValueChanged += OnWeaponChanged;
            heavyWeapon.OnValueChanged += OnWeaponChanged;
            closeRangeWeapon.OnValueChanged += OnWeaponChanged;
        }

        public void Drop()
        {
            if (SelectedMode == Mode.Weapon)
                DropCurrentWeapon();
            else if (SelectedMode == Mode.Item)
                DropCurrentItem();
        }

        public void PickUpObject(GameObject go)
        {
            if (go.TryGetComponent(out BaseWeapon weapon))
                AddWeapon(weapon);
            else if (go.TryGetComponent(out BaseItem item))
                AddItem(item);
        }

        private void HandleModeRenderers(Mode mode, bool reset = false)
        {
            if (!IsServer)
                throw new NotServerException();


            if (reset)
            {
                foreach (var container in itemRegistry)
                {
                    foreach (var item in container.Value)
                    {
                        item.SwitchRender(false);
                    }
                }

                foreach (var w in Weapons)
                {
                    w.GetComponent<BaseWeapon>().SwitchRender(false);
                }
            }

            switch (mode)
            {
                case Mode.Item:
                    if (SelectedItem != null)
                        SelectedItem.SwitchRender(true);
                    if (CurrentWeapon != null)
                        CurrentWeapon.SwitchRender(false);
                    break;
                case Mode.Weapon:
                    if (SelectedItem != null)
                        SelectedItem.SwitchRender(false);
                    if (CurrentWeapon != null)
                        CurrentWeapon.SwitchRender(true);
                    SetHandTargetsClientRpc();


                    break;
            }
        }

        [ClientRpc]
        private void SetHandTargetsClientRpc()
        {
            if (CurrentWeapon != null && SelectedMode == Mode.Weapon)
                Player.SetHandTargets(CurrentWeapon.RightHandTarget, CurrentWeapon.LeftHandTarget);
        }


        [ServerRpc]
        private void UpdateModeServerRpc(Mode value)
        {
            if (value != SelectedMode)
            {
                selectedMode.Value = value;
                HandleModeRenderers(value);
            }
        }

        /// <summary>
        ///     changes the inventory mode
        /// </summary>
        /// <param name="newMode"></param>
        /// <returns>whether the mode was changed</returns>
        public void ChangeMode(Mode newMode)
        {
            UpdateModeServerRpc(newMode);
        }
    }
}
