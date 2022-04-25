using System;
using System.Linq;
using Abilities;
using Items;
using model.Network;
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

        private readonly NetworkVariable<NetworkBehaviourReference> playerReference =
            new NetworkVariable<NetworkBehaviourReference>();


        /**
         * <value>the bound player <seealso cref="BasePlayer"/></value>
         */
        public BasePlayer Player
        {
            set
            {
                UpdatePlayerReferenceServerRpc(new NetworkBehaviourReference(value));
                AbilityTree = new AbilityTree(value, value.RootAbility);
            }
            get => playerReference.Value.TryGet(out BasePlayer p) ? p : null;
        }

        private readonly NetworkVariable<Mode> selectedMode = new NetworkVariable<Mode>();

        public Mode SelectedMode
        {
            get => selectedMode.Value;
            private set => UpdateModeServerRpc(value);
        }

        public AbilityTree AbilityTree { get; private set; }


        private void Awake()
        {
            selectedMode.OnValueChanged += (old, value) => HandleModeRenderers(value);
            itemRegistry.OnValueChange += ev =>
            {
                switch (ev.Type)
                {
                    case NetworkItemRegistry.ItemRegistryEvent.EventType.Push:
                    {
                        if (SelectedItemType == null)
                            UpdateSelectedItemTypeServerRpc(ev.ObjType);
                        ev.itemRef.TryGet(out BaseItem item);
                        item.SwitchRender(false);
                    }
                        break;
                    case NetworkItemRegistry.ItemRegistryEvent.EventType.Pop:
                    {
                        if (ev.index == 0)
                        {
                            if (!itemRegistry.Any())
                            {
                                SelectedMode = Mode.Weapon;
                            }
                            else
                            {
                                SelectedItemType = itemRegistry.First().Key;
                            }
                        }
                    }
                        break;
                    case NetworkItemRegistry.ItemRegistryEvent.EventType.Full:

                        foreach (var pair in itemRegistry)
                        {
                            foreach (var item in pair.Value)
                            {
                                item.SwitchRender(false);
                            }
                        }
                        break;
                }
                HandleModeRenderers(SelectedMode);
            };
        }

        void Start()
        {
            if (ItemWheel == null)
                itemWheel = gameObject.AddComponent<ItemWheel>();


            HandleModeRenderers(SelectedMode);
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

        private void HandleModeRenderers(Mode mode)
        {
            switch (mode)
            {
                case Mode.Item:
                    // ReSharper disable once PossibleNullReferenceException
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


        [ServerRpc(RequireOwnership = false)]
        private void UpdatePlayerReferenceServerRpc(NetworkBehaviourReference playerRef)
        {
            this.playerReference.Value = playerRef;
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
        public bool ChangeMode(Mode newMode)
        {
            UpdateModeServerRpc(newMode);
            return true;
        }
    }
}