using System;
using System.Collections.Generic;
using model;
using PlayerControllers;
using Unity.Netcode;
using UnityEngine;

namespace Items
{
    public abstract class BaseItem : NetworkBehaviour, IItem
    {
        public static Dictionary<Type, int> MaxDictionary = new Dictionary<Type, int>
        {
            {typeof(FuelBooster), 3}
        };

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
        public BasePlayer Player { get; set; }

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

        protected abstract void Setup();

        protected abstract void OnConsume();
        protected abstract void TearDown();
    }
}