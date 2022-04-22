using PlayerControllers;
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


        public bool ItemSelected => !(SelectedItemType is null);


        [ServerRpc(RequireOwnership = false)]
        private void UpdatePlayerReferenceServerRpc(NetworkBehaviourReference playerRef)
        {
            this.playerReference.Value = playerRef;
        }
    }
}