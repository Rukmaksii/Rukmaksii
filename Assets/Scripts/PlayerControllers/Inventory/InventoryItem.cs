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

        private BaseItem lastItem = null;

        public Type SelectedItemType
        {
            get =>
                BaseItem.ItemInfos.Keys
                    .FirstOrDefault(v => BaseItem.GetBaseItemHashCode(v) == selectedItemType.Value);

            set => UpdateSelectedItemTypeServerRpc(BaseItem.GetBaseItemHashCode(value));
        }

        private ItemWheel itemWheel;

        public ItemWheel ItemWheel => itemWheel;

        [CanBeNull]
        public BaseItem SelectedItem => SelectedItemType is null ? null : itemRegistry[SelectedItemType].Top;

        // ReSharper disable Unity.PerformanceAnalysis
        /**
                 * <summary>adds an instantiated item to the inventory</summary>
                 * <param name="item">a BaseItem to be added</param>
                 */
        public void AddItem(BaseItem item)
        {
            if (item.State != ItemState.Clean || !itemRegistry[item.GetType()].CanPush)
                return;

            if (IsOwner)
            {
                if (SelectedItem != null)
                    item.SwitchRender(false);
                else
                    SelectedItemType = item.GetType();
                itemRegistry[item.GetType()].Push(item);
                AddItemServerRpc(new NetworkBehaviourReference(item));
                HandleModeRenderers(SelectedMode);
                if (itemWheel == null)
                    itemWheel = gameObject.AddComponent<ItemWheel>();
                itemWheel.AddItem(item);
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

        [ServerRpc]
        private void AddItemServerRpc(NetworkBehaviourReference itemRef)
        {
            itemRef.TryGet(out BaseItem item);
            item.PickUp(Player);
        }

        [ClientRpc]
        private void AddItemClientRpc(NetworkBehaviourReference itemRef, ClientRpcParams p = default)
        {
            itemRef.TryGet(out BaseItem item);
            if (SelectedItem != null)
                item.SwitchRender(false);
            else
                SelectedItemType = item.GetType();
            itemRegistry[item.GetType()].Push(item);
            HandleModeRenderers(SelectedMode);
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
            {
                DropItemServerRpc(new NetworkBehaviourReference(item));
                itemWheel.RemoveItem(item);
            }
        }

        public void UseItem()
        {
            if (!IsOwner || SelectedItem == null || lastItem != null && !lastItem.IsReady)
                return;

            var container = itemRegistry[SelectedItemType];
            var item = container.Pop();
            if (container.Count <= 0)
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

            item.Consume();
            lastItem = item;
        }

        [ServerRpc]
        private void DropItemServerRpc(NetworkBehaviourReference itemRef)
        {
            itemRef.TryGet(out BaseItem item);
            var container = itemRegistry[item.GetType()];
            if (container.Count <= 0)
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

            item.Drop();
        }

        [ServerRpc]
        private void UpdateSelectedItemTypeServerRpc(long value)
        {
            if (SelectedItem != null)
                SelectedItem.SwitchRender(false);
            selectedItemType.Value = value;
            if (SelectedItem != null)
                SelectedItem.SwitchRender(true);
        }
    }
}