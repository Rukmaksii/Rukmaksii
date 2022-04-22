using System;
using System.Collections.Generic;
using model;
using PlayerControllers;
using Unity.Netcode;
using Unity.Netcode.Samples;
using UnityEngine;

namespace Items
{
    public struct ItemInfo
    {
        public string Name;
        public ItemCategory Category;
        public int MaxCount;

        public new long GetHashCode()
        {
            string toParse = Name + new string((char) Category, MaxCount);
            int i;
            long result = 0;
            for (i = 0; i + 7 < toParse.Length; i += 8)
            {
                long part = 0;
                for (int j = i; j < i + 8; j++)
                {
                    part <<= 8;
                    part += (byte) toParse[j];
                }

                result ^= part;
            }

            long end = 0;
            for (; i < toParse.Length; i++)
            {
                end <<= 8;
                end += (byte) toParse[i];
            }

            return result ^ end;
        }
    }


    [RequireComponent(typeof(NetworkObject))]
    [RequireComponent(typeof(ClientNetworkTransform))]
    public abstract class BaseItem : NetworkBehaviour, IItem, IPickable
    {
        public static readonly Dictionary<Type, ItemInfo> ItemInfos = new Dictionary<Type, ItemInfo>
        {
            {
                typeof(FuelBooster), new ItemInfo()
                {
                    Name = "Fuel Booster",
                    Category = ItemCategory.Other,
                    MaxCount = 3
                }
            }
        };

        private NetworkVariable<NetworkBehaviourReference> playerReference =
            new NetworkVariable<NetworkBehaviourReference>();

        public BasePlayer Player
        {
            set =>
                UpdatePlayerServerRpc(value is null
                    ? new NetworkBehaviourReference()
                    : new NetworkBehaviourReference(value));

            get => playerReference.Value.TryGet(out BasePlayer res) ? res : null;
        }

        public bool IsOwned => !(Player is null);

        public ItemInfo Info => ItemInfos[GetType()];

        public abstract float Duration { get; protected set; }

        private float consumedTime = 0;

        private NetworkVariable<ItemState> itemState = new NetworkVariable<ItemState>(ItemState.Clean);

        public ItemState State
        {
            get => itemState.Value;
            private set => UpdateStateServerRpc(value);
        }

        // to avoid latency on status changed
        private bool started = false;


        public string Name => Info.Name;
        public ItemCategory Category => Info.Category;


        private void Awake()
        {
            if (!ItemInfos.ContainsKey(this.GetType()))
                throw new KeyNotFoundException($"item {Name} was not referenced in BaseItem::ItemInfos");
        }

        private void Update()
        {
            if (!IsOwner || State == ItemState.Consumed || consumedTime < 0)
                return;
            if (!started)
            {
                if (State == ItemState.Consuming)
                {
                    Setup();
                    started = true;
                }

                return;
            }

            if (Duration > 0 && consumedTime > Duration)
            {
                EndConsumption();
                return;
            }

            OnConsume();
            consumedTime += Time.deltaTime;
        }

        public void Consume()
        {
            if (State == ItemState.Clean)
                State = ItemState.Consuming;
        }

        /// <summary>
        ///     stops consumption for infinite life items
        /// </summary>
        protected void EndConsumption()
        {
            State = ItemState.Consumed;
            consumedTime = -1;
            TearDown();
        }

        [ServerRpc]
        private void UpdateStateServerRpc(ItemState value)
        {
            itemState.Value = value;
        }

        [ServerRpc(RequireOwnership = false)]
        private void UpdatePlayerServerRpc(NetworkBehaviourReference playerRef)
        {
            this.playerReference.Value = playerRef;
        }

        protected abstract void Setup();

        protected abstract void OnConsume();
        protected abstract void TearDown();

        public void PickUp(BasePlayer player)
        {
            if (!IsServer)
                throw new NotServerException();
            Player = player;
            NetworkObject.ChangeOwnership(Player.OwnerClientId);
            NetworkObject.TrySetParent(Player.transform);
        }

        public void Drop()
        {
            if (!IsServer)
                throw new NotServerException();
            transform.SetParent(null);
            transform.SetPositionAndRotation(Player.transform.position, Player.transform.rotation);
            NetworkObject.ChangeOwnership(NetworkManager.ServerClientId);
            Player = null;
        }

        public static long GetBaseItemHashCode(Type baseItemType)
        {
            if (!baseItemType.IsSubclassOf(typeof(BaseItem)))
                throw new ArgumentException("could not get persistent hash code for non base item instance");

            return ItemInfos[baseItemType].GetHashCode();
        }
    }
}