using System;
using System.Linq;
using Items;
using JetBrains.Annotations;
using model.Network;
using Unity.Netcode;
using UnityEngine;

namespace model
{
    public partial class Inventory
    {
        private readonly NetworkItemRegistry itemRegistry =
            new NetworkItemRegistry(writePermission: NetworkVariableWritePermission.Server);

        private readonly NetworkVariable<long> selectedItemType = new NetworkVariable<long>();

        private BaseItem lastItem = null;

        public NetworkItemRegistry ItemRegistry => itemRegistry;

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
            if (IsOwner || IsServer)

            {
                AddItemServerRpc(new NetworkBehaviourReference(item));
                if (itemWheel == null)
                    itemWheel = gameObject.AddComponent<ItemWheel>();
            }
        }

        [ServerRpc(RequireOwnership = false)]
        private void AddItemServerRpc(NetworkBehaviourReference itemRef)
        {
            itemRef.TryGet(out BaseItem item);

            if (item.State != ItemState.Clean || !itemRegistry[item.GetType()].CanPush)
            {
                Player.Money += item.Price;
                item.gameObject.GetComponent<NetworkObject>().Despawn();
                return;
            }

            item.PickUp(Player);
            if (!itemRegistry.Any())
                SelectedItemType = item.GetType();
            else
                // ReSharper disable once PossibleNullReferenceException
                SelectedItem.SwitchRender(false);
            itemRegistry[item.GetType()].Push(item);
            item.SwitchRender(false);
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
            DropItemServerRpc();
        }

        public void UseItem()
        {
            UseItemServerRpc();
        }

        [ServerRpc]
        private void UseItemServerRpc()
        {
            if (SelectedItem == null || lastItem != null && !lastItem.IsReady)
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

            HandleModeRenderers(SelectedMode);

            item.Consume();
            item.SwitchRender(true);
            lastItem = item;
        }

        [ServerRpc]
        private void DropItemServerRpc()
        {
            var container = itemRegistry[SelectedItemType];
            if (container.TryPop(out BaseItem item))
                item.Drop();
            else
                return;

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

            HandleModeRenderers(SelectedMode);
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