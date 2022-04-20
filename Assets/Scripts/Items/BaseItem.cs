using System;
using System.Collections.Generic;
using model;
using PlayerControllers;
using Unity.Netcode;
using Unity.Netcode.Samples;
using UnityEngine;

namespace Items
{
    [RequireComponent(typeof(NetworkObject))]
    [RequireComponent(typeof(ClientNetworkTransform))]
    public abstract class BaseItem : NetworkBehaviour, IItem, IPickable
    {
        public static Dictionary<Type, int> MaxDictionary = new Dictionary<Type, int>
        {
            {typeof(FuelBooster), 3}
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

        public abstract ItemCategory Type { get; }

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


        public abstract string Name { get; }

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
            NetworkObject.ChangeOwnership(NetworkManager.Singleton.ServerClientId);
            Player = null;
        }
    }
}