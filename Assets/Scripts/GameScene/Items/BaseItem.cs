using System;
using System.Collections.Generic;
using GameScene.model;
using GameScene.PlayerControllers.BasePlayer;
using Unity.Netcode;
using Unity.Netcode.Samples;
using UnityEngine;

namespace GameScene.Items
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
                typeof(Grenade), new ItemInfo("Grenade", ItemCategory.Attack, 10, "Items/Grenade")
            },
            {
                typeof(Money), new ItemInfo("Money", ItemCategory.Other, 0, "Items/Money")
            },
            {
                typeof(Bandage), new ItemInfo("Bandage", ItemCategory.Heal, 5, "Items/Bandage")
            },
            {
                typeof(Medkit), new ItemInfo("Medkit", ItemCategory.Heal, 1, "Items/Medkit")
            }
        };

        public string InteractableName => Name;


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

        public bool IsInteractable => !(Player is null);

        public ItemInfo Info => ItemInfos[GetType()];

        public abstract float Duration { get; }

        public abstract int Price { get; }

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
            SwitchRenderers(renderState.Value);

            playerReference.OnValueChanged += (_, val) => SwitchColliders(!IsInteractable);
            SwitchColliders(!IsInteractable);
        }

        private void Update()
        {
            if (!IsSpawned)
                return;
            if (!started)
            {
                GetComponent<Rigidbody>().isKinematic = IsInteractable;
                if (IsOwner && IsInteractable)
                    transform.localPosition = Player.transform.InverseTransformPoint(Player.WeaponContainer.position);
            }

            if (!IsServer)
                return;

            if (!IsInteractable ||
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
            NetworkObject.Despawn();
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

        public bool Interact(BasePlayer player)
        {
            if (!IsServer)
                throw new NotServerException();
            Player = player;
            NetworkObject.ChangeOwnership(player.OwnerClientId);
            NetworkObject.TrySetParent(Player.transform);
            return true;
        }

        public void UnInteract()
        {
            if (!IsServer)
                throw new NotServerException();
            NetworkObject.ChangeOwnership(NetworkManager.ServerClientId);
            transform.SetParent(null);
            transform.SetPositionAndRotation(Player.transform.position, Player.transform.rotation);
            Player = null;
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
