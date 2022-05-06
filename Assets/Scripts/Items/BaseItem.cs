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
        private string spritePath;
        public Sprite Sprite => Resources.Load<Sprite>(spritePath);
        public string Name;
        public ItemCategory Category;
        public int MaxCount;

        public ItemInfo(string name, ItemCategory category, int maxCount, string spritePath)
        {
            Name = name;
            Category = category;
            MaxCount = maxCount;
            this.spritePath = spritePath;
        }

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
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(Collider))]
    public abstract class BaseItem : NetworkBehaviour, IItem, IPickable
    {
        public static readonly Dictionary<Type, ItemInfo> ItemInfos = new Dictionary<Type, ItemInfo>
        {
            {
                typeof(FuelBooster), new ItemInfo("Fuel Booster", ItemCategory.Other, 3, "Items/JerryCan")
            },
            {
                typeof(Grenade), new ItemInfo("Grenade", ItemCategory.Other, 10, "Items/Grenade")
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

            get => IsSpawned && playerReference.Value.TryGet(out BasePlayer res) ? res : null;
        }

        public bool IsOwned => !(Player is null);

        public ItemInfo Info => ItemInfos[GetType()];

        public abstract float Duration { get; protected set; }
        
        public abstract int Price { get; set; }

        /// <summary>
        ///     the time after which the player can use another item
        /// </summary>
        protected virtual float ReadyCooldown { get; } = 0f;


        private NetworkVariable<bool> isReady = new NetworkVariable<bool>();

        /// <summary>
        ///     a flag indicating whether the player can use another item
        /// </summary>
        public bool IsReady => isReady.Value;

        private float consumedTime = 0;

        private NetworkVariable<ItemState> itemState = new NetworkVariable<ItemState>(ItemState.Clean);

        private NetworkVariable<bool> renderState = new NetworkVariable<bool>(true);

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
            if (!ItemInfos.ContainsKey(GetType()))
                throw new KeyNotFoundException($"item {GetType().Name} was not referenced in BaseItem::ItemInfos");
        }

        private void Start()
        {
            renderState.OnValueChanged += (old, val) => SwitchRenderers(val);
            playerReference.OnValueChanged += (_, val) => SwitchColliders(!IsOwned);
        }

        private void Update()
        {
            if (!IsSpawned)
                return;
            if (!started)
            {
                GetComponent<Rigidbody>().isKinematic = IsOwned;
                if (IsOwner && IsOwned)
                    transform.localPosition = Player.transform.InverseTransformPoint(Player.WeaponContainer.position);
            }

            if (!IsServer)
                return;

            if (!IsOwned ||
                !IsServer ||
                State == ItemState.Consumed ||
                consumedTime < 0)
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
            if (consumedTime >= ReadyCooldown)
                isReady.Value = true;
        }

        public void Consume()
        {
            if (!IsServer)
                throw new NotServerException();
            if (State == ItemState.Clean)
                State = ItemState.Consuming;
        }

        /// <summary>
        ///     stops consumption for infinite life items
        /// </summary>
        protected void EndConsumption()
        {
            if (!IsServer)
                throw new NotServerException();
            State = ItemState.Consumed;
            consumedTime = -1;
            TearDown();
            DespawnServerRpc();
        }

        [ServerRpc(RequireOwnership = false)]
        private void UpdateStateServerRpc(ItemState value)
        {
            itemState.Value = value;
        }

        [ServerRpc(RequireOwnership = false)]
        private void UpdatePlayerServerRpc(NetworkBehaviourReference playerRef)
        {
            this.playerReference.Value = playerRef;
            if (playerRef.TryGet(out BasePlayer _))
                SwitchColliders(false);
            else
                SwitchColliders(true);
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
            NetworkObject.ChangeOwnership(NetworkManager.ServerClientId);
            transform.SetParent(null);
            transform.SetPositionAndRotation(Player.transform.position, Player.transform.rotation);
            Player = null;
        }

        [ServerRpc]
        private void DespawnServerRpc()
        {
            NetworkObject.Despawn();
        }

        public void SwitchRender(bool render)
        {
            if (!IsServer)
                throw new NotServerException("only server can switch render");


            renderState.Value = render;
            SwitchRenderers(render);
        }

        private void SwitchRenderers(bool render)
        {
            foreach (var renderer in GetComponentsInChildren<MeshRenderer>())
                renderer.enabled = render;
        }

        private void SwitchColliders(bool collide)
        {
            foreach (var collider in GetComponentsInChildren<Collider>())
                collider.enabled = collide;
        }

        public static long GetBaseItemHashCode(Type baseItemType)
        {
            if (!baseItemType.IsSubclassOf(typeof(BaseItem)))
                throw new ArgumentException("could not get persistent hash code for non base item instance");

            return ItemInfos[baseItemType].GetHashCode();
        }
    }
}