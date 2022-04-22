﻿using PlayerControllers;
using Unity.Netcode;

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
            set => UpdatePlayerReferenceServerRpc(new NetworkBehaviourReference(value));
            get => playerReference.Value.TryGet(out BasePlayer p) ? p : null;
        }

        private readonly NetworkVariable<Mode> selectedMode = new NetworkVariable<Mode>(Mode.Weapon);

        public Mode SelectedMode
        {
            get => selectedMode.Value;
            private set => UpdateModeServerRpc(value);
        }


        void Start()
        {
            selectedMode.OnValueChanged += (old, value) => HandleModeRenderers(value);
            HandleModeRenderers(SelectedMode);
        }

        private void HandleModeRenderers(Mode mode)
        {
            if (SelectedItem == null)
                return;
            switch (mode)
            {
                case Mode.Item:
                    SelectedItem.SwitchRender(true);
                    CurrentWeapon.SwitchRender(false);
                    break;
                case Mode.Weapon:
                    SelectedItem.SwitchRender(false);
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
            selectedMode.Value = value;
        }

        public void ChangeMode(Mode newMode)
        {
            UpdateModeServerRpc(newMode);
        }
    }
}