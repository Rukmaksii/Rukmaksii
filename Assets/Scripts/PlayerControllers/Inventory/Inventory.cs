using Abilities;
using Items;
using PlayerControllers;
using Unity.Netcode;
using UnityEngine;
using Weapons;

namespace model
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


        private BasePlayer _player;

        /**
         * <value>the bound player <seealso cref="BasePlayer"/></value>
         */
        public BasePlayer Player
        {
            get
            {
                if (_player is null)
                {
                    _player = GetComponent<BasePlayer>();
                    AbilityTree = new AbilityTree(_player, _player.RootAbility);
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
                    break;
            }
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