using System;
using System.Linq;
using Items;
using JetBrains.Annotations;
using model.Network;
using Unity.Netcode;

namespace model
{
    public partial class Inventory
    {
        private readonly NetworkItemRegistry itemRegistry =
            new NetworkItemRegistry(writePermission: NetworkVariableWritePermission.Owner);

        private readonly NetworkVariable<long> selectedItemType = new NetworkVariable<long>();

        public Type SelectedItemType
        {
            get =>
                BaseItem.ItemInfos.Keys
                    .FirstOrDefault(v => BaseItem.GetBaseItemHashCode(v) == selectedItemType.Value);

            set => UpdateSelectedItemTypeServerRpc(BaseItem.GetBaseItemHashCode(value));
        }

        [CanBeNull]
        public BaseItem SelectedItem => SelectedItemType is null ? null : itemRegistry[SelectedItemType].Top;

        /**
                 * <summary>adds an instantiated item to the inventory</summary>
                 * <param name="item">a BaseItem to be added</param>
                 */
        public void AddItem(BaseItem item)
        {
            if (item.State != ItemState.Clean || !itemRegistry[item.GetType()].CanPush)
                return;

            if (IsOwner && itemRegistry[item.GetType()].TryPush(item))
            {
                AddItemServerRpc(new NetworkBehaviourReference(item));
            }
            else if (IsServer)
            {
                item.PickUp(Player);
                ClientRpcParams p = new ClientRpcParams
                {
                    Send = new ClientRpcSendParams
                    {
                        TargetClientIds = new[] {Player.OwnerClientId}
                    }
                };
                AddItemClientRpc(new NetworkBehaviourReference(item), p);
            }
        }

        [ServerRpc(RequireOwnership = false)]
        private void AddItemServerRpc(NetworkBehaviourReference itemRef)
        {
            itemRef.TryGet(out BaseItem item);
            item.PickUp(Player);
        }

        [ClientRpc]
        private void AddItemClientRpc(NetworkBehaviourReference itemRef, ClientRpcParams p = default)
        {
            itemRef.TryGet(out BaseItem item);
            itemRegistry[item.GetType()].Push(item);
        }

        public NetworkItemRegistry.ItemContainer GetItemContainer<TForMethod>() where TForMethod : BaseItem
        {
            return itemRegistry[typeof(TForMethod)];
        }

        /**
                 * <summary>drops the current selected Item </summary>
                 * <param name="item">a BaseItem to be removed</param>
                 */
        public void DropCurrentItem()
        {
            if (SelectedMode != Mode.Item)
                return;
            if (itemRegistry[SelectedItemType].TryPop(out BaseItem item))
                DropItemServerRpc(new NetworkBehaviourReference(item));
        }

        [ServerRpc]
        private void DropItemServerRpc(NetworkBehaviourReference itemRef)
        {
            itemRef.TryGet(out BaseItem item);
            item.Drop();
        }

        [ServerRpc]
        private void UpdateSelectedItemTypeServerRpc(long value)
        {
            selectedItemType.Value = value;
        }
    }
}