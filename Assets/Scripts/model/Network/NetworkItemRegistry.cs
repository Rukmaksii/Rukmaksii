using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Items;
using JetBrains.Annotations;
using Unity.Netcode;

namespace model.Network
{
    public class NetworkItemRegistry : NetworkVariableBase,
        IEnumerable<KeyValuePair<Type, NetworkItemRegistry.ItemContainer>>
    {
        private List<ItemRegistryEvent> dirtyEvents = new List<ItemRegistryEvent>();

        // binds item type hash code to list of item references
        private Dictionary<long, List<NetworkBehaviourReference>> data =
            new Dictionary<long, List<NetworkBehaviourReference>>();

        public ItemContainer this[Type itemType] =>
            new ItemContainer(BaseItem.ItemInfos[itemType].MaxCount, itemType, this);

        public delegate void OnValueChangeDelegate(ItemRegistryEvent @event);

        public OnValueChangeDelegate OnValueChange = null;


        public NetworkItemRegistry()
        {
        }

        public NetworkItemRegistry(NetworkVariableReadPermission readPermission = DefaultReadPerm,
            NetworkVariableWritePermission writePermission = DefaultWritePerm) : base(readPermission, writePermission)
        {
        }

        public override void ResetDirty()
        {
            base.ResetDirty();
            dirtyEvents.Clear();
        }

        public override bool IsDirty()
        {
            return base.IsDirty() || dirtyEvents.Count > 0;
        }


        public override void WriteDelta(FastBufferWriter writer)
        {
            if (base.IsDirty())
            {
                writer.WriteValueSafe((ushort) 1);
                writer.WriteValueSafe(ItemRegistryEvent.EventType.Full);
                WriteField(writer);
                return;
            }

            writer.WriteValueSafe((ushort) dirtyEvents.Count);
            foreach (var dirtyEvent in dirtyEvents)
            {
                writer.WriteValueSafe(dirtyEvent.Type);
                switch (dirtyEvent.Type)
                {
                    case ItemRegistryEvent.EventType.RemoveAt:
                        // remove at
                        writer.WriteValueSafe(dirtyEvent.ObjType);
                        writer.WriteValueSafe(dirtyEvent.index);
                        break;
                    case ItemRegistryEvent.EventType.Push:
                        // push item at key
                        writer.WriteValueSafe(dirtyEvent.ObjType);
                        writer.WriteNetworkSerializable(dirtyEvent.itemRef);
                        break;
                    case ItemRegistryEvent.EventType.Pop:
                        // pop item at key
                        writer.WriteValueSafe(dirtyEvent.ObjType);
                        break;
                }
            }
        }

        public override void WriteField(FastBufferWriter writer)
        {
            // writes the number of different items
            writer.WriteValueSafe((ushort) data.Count);
            foreach (var pair in data)
            {
                long objType = pair.Key;
                List<NetworkBehaviourReference> objRefs = pair.Value;
                writer.WriteValueSafe(objType);
                writer.WriteValueSafe(objRefs.Count);
                foreach (var objRef in objRefs)
                {
                    writer.WriteNetworkSerializable(objRef);
                }
            }
        }

        public override void ReadField(FastBufferReader reader)
        {
            // clears data
            data.Clear();

            // fetches container count
            reader.ReadValueSafe(out ushort containerCount);
            for (int i = 0; i < containerCount; i++)
            {
                // fetches type hashcode
                reader.ReadValueSafe(out long objType);
                // fetches item count
                reader.ReadValueSafe(out int itemCount);
                if (!data.ContainsKey(objType))
                    data[objType] = new List<NetworkBehaviourReference>();

                for (int j = 0; j < itemCount; j++)
                {
                    reader.ReadNetworkSerializable(out NetworkBehaviourReference objRef);
                    data[objType].Add(objRef);
                }
            }
        }

        public override void ReadDelta(FastBufferReader reader, bool keepDirtyDelta)
        {
            reader.ReadValueSafe(out ushort deltaCount);
            for (int i = 0; i < deltaCount; i++)
            {
                reader.ReadValueSafe(out ItemRegistryEvent.EventType eventType);
                switch (eventType)
                {
                    case ItemRegistryEvent.EventType.RemoveAt:
                    {
                        reader.ReadValueSafe(out long objType);
                        reader.ReadValueSafe(out int index);

                        NetworkBehaviourReference itemRef = data[objType][index];
                        data[objType].RemoveAt(index);

                        if (data[objType].Count <= 0)
                            data.Remove(objType);

                        var regEvent = new ItemRegistryEvent()
                        {
                            Type = eventType,
                            ObjType = objType,
                            index = index,
                            itemRef = itemRef
                        };
                        OnValueChange?.Invoke(regEvent);
                        if (keepDirtyDelta)
                        {
                            dirtyEvents.Add(regEvent);
                        }
                    }
                        break;
                    case ItemRegistryEvent.EventType.Push:
                    {
                        reader.ReadValueSafe(out long objType);
                        reader.ReadNetworkSerializable(out NetworkBehaviourReference itemRef);
                        if (!data.ContainsKey(objType))
                            data[objType] = new List<NetworkBehaviourReference>();
                        data[objType].Add(itemRef);

                        var regEvent = new ItemRegistryEvent()
                        {
                            Type = eventType,
                            ObjType = objType,
                            itemRef = itemRef
                        };

                        OnValueChange?.Invoke(regEvent);
                        if (keepDirtyDelta)
                        {
                            dirtyEvents.Add(regEvent);
                        }
                    }
                        break;
                    case ItemRegistryEvent.EventType.Pop:
                    {
                        reader.ReadValueSafe(out long objType);
                        int index = data[objType].Count - 1;
                        NetworkBehaviourReference itemRef = data[objType][index];
                        data[objType].RemoveAt(index);

                        if (data[objType].Count <= 0)
                            data.Remove(objType);

                        var regEvent = new ItemRegistryEvent()
                        {
                            Type = eventType,
                            ObjType = objType,
                            index = index,
                            itemRef = itemRef
                        };

                        OnValueChange?.Invoke(regEvent);
                        if (keepDirtyDelta)
                        {
                            dirtyEvents.Add(regEvent);
                        }
                    }
                        break;
                    case ItemRegistryEvent.EventType.Clear:
                    {
                        data.Clear();

                        var regEvent = new ItemRegistryEvent()
                        {
                            Type = eventType
                        };

                        OnValueChange?.Invoke(regEvent);
                        if (keepDirtyDelta)
                        {
                            dirtyEvents.Add(regEvent);
                        }
                    }
                        break;
                    case ItemRegistryEvent.EventType.Full:
                    {
                        ReadField(reader);
                        var regEvent = new ItemRegistryEvent()
                        {
                            Type = eventType
                        };

                        ResetDirty();

                        OnValueChange?.Invoke(regEvent);
                    }
                        break;
                }
            }
        }


        private void RemoveAt(long objectType, int index)
        {
            if (!CanClientWrite(NetworkManager.Singleton.LocalClientId))
                throw new InvalidOperationException("Client cannot write to item registry");
            data[objectType].RemoveAt(index);
            if (data[objectType].Count <= 0)
                data.Remove(objectType);
            dirtyEvents.Add(new ItemRegistryEvent()
            {
                Type = index == data[objectType].Count
                    ? ItemRegistryEvent.EventType.Pop
                    : ItemRegistryEvent.EventType.RemoveAt,
                ObjType = objectType,
                index = index
            });
        }

        private void AddItem(long objectType, NetworkBehaviourReference itemRef)
        {
            if (!CanClientWrite(NetworkManager.Singleton.LocalClientId))
                throw new InvalidOperationException("Client cannot write to item registry");
            if (!data.ContainsKey(objectType))
                data[objectType] = new List<NetworkBehaviourReference>();
            data[objectType].Add(itemRef);
            dirtyEvents.Add(new ItemRegistryEvent()
            {
                Type = ItemRegistryEvent.EventType.Push,
                ObjType = objectType,
                itemRef = itemRef
            });
        }

        public bool ContainsKey(Type key)
        {
            return data.ContainsKey(BaseItem.GetBaseItemHashCode(key));
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public IEnumerator<KeyValuePair<Type, ItemContainer>> GetEnumerator()
        {
            List<KeyValuePair<Type, ItemContainer>> res = new List<KeyValuePair<Type, ItemContainer>>();
            foreach (var pair in BaseItem.ItemInfos)
            {
                var container = this[pair.Key];
                if (container.Count > 0)
                    res.Add(new KeyValuePair<Type, ItemContainer>(pair.Key, container));
            }

            return res.GetEnumerator();
        }

        /// <summary>
        ///     a container for items of the same type 
        /// </summary>
        /// <remarks>only allowed writers can use non-pure exposed API <br/> therefore, non-allowed writers can only access <see cref="GetEnumerable"/></remarks>
        public class ItemContainer : IEnumerable<BaseItem>
        {
            public readonly int MaxCount;
            public readonly Type ItemType;
            private readonly NetworkItemRegistry registry;
            private List<NetworkBehaviourReference> items => registry.data[BaseItem.GetBaseItemHashCode(ItemType)];
            private bool ContainerExists => registry.ContainsKey(ItemType);

            public int Count => ContainerExists ? Data().Count() : 0;

            /// <summary>
            ///     the result of Peek function without modifying the data struct
            /// </summary>
            public BaseItem Top => Count > 0 ? Data().Last() : null;

            internal ItemContainer(int maxCount, Type itemType, NetworkItemRegistry registry)
            {
                MaxCount = maxCount;
                ItemType = itemType;
                this.registry = registry;
            }

            private void CleanData()
            {
                // awaiting for push operation
                if (!ContainerExists)
                    return;
                for (int i = 0; i < items.Count; i++)
                {
                    if (items[i].TryGet(out BaseItem baseItem) && baseItem.State == ItemState.Clean)
                        continue;
                    registry.RemoveAt(BaseItem.GetBaseItemHashCode(ItemType), i--);
                }
            }

            /// <summary>
            ///     pops an item from the stack
            /// </summary>
            /// <returns>the popped item</returns>
            /// <exception cref="IndexOutOfRangeException">if there is no item to pop</exception>
            /// <exception cref="InvalidOperationException">if the caller is not an allowed writer</exception>
            /// <seealso cref="TryPop"/>
            public BaseItem Pop()
            {
                CleanData();
                int count = Count;
                if (count <= 0)
                    throw new IndexOutOfRangeException("could not pop from an empty stack");

                items[count - 1].TryGet(out BaseItem result);
                registry.RemoveAt(BaseItem.GetBaseItemHashCode(ItemType), Count - 1);

                return result;
            }

            /// <summary>
            ///     peeks the top of the stack
            /// </summary>
            /// <returns>the peaked base item</returns>
            /// <remarks>peeking does not modify the stack top</remarks>
            /// <exception cref="IndexOutOfRangeException">if the stack is empty</exception>
            /// <exception cref="InvalidOperationException">if the caller is not an allowed writer</exception>
            /// <seealso cref="TryPeek"/>
            public BaseItem Peek()
            {
                CleanData();
                int count = Count;
                if (count <= 0)
                    throw new IndexOutOfRangeException("could not peek from an empty stack");

                items[count - 1].TryGet(out BaseItem result);
                return result;
            }

            /// <summary>
            ///     tries peeking the top of the stack
            /// </summary>
            /// <param name="result">the peeked to</param>
            /// <returns>true if the stack was not empty</returns>
            /// <remarks>peeking does not modify the stack top</remarks>
            /// <exception cref="InvalidOperationException">if the caller is not an allowed writer</exception>
            /// <seealso cref="Peek"/>
            public bool TryPeek(out BaseItem result)
            {
                try
                {
                    result = Peek();
                }
                catch (IndexOutOfRangeException)
                {
                    result = null;
                    return false;
                }

                return result != null;
            }


            /// <summary>
            ///     tries popping the top of the stack
            /// </summary>
            /// <param name="result">the popped top of the stack</param>
            /// <returns>true if the stack was not empty</returns>
            /// <exception cref="InvalidOperationException">if the caller is not an allowed writer</exception>
            public bool TryPop(out BaseItem result)
            {
                try
                {
                    result = Pop();
                }
                catch (IndexOutOfRangeException)
                {
                    result = null;
                    return false;
                }

                return result != null;
            }

            /// <summary>
            ///     pushes an item to the stack
            /// </summary>
            /// <param name="baseItem">the item to push</param>
            /// <exception cref="ArgumentException">if item type is invalid</exception>
            /// <exception cref="IndexOutOfRangeException">if excedeed container max count</exception>
            /// <exception cref="InvalidOperationException">if the caller is not an allowed writer</exception>
            /// <seealso cref="TryPush"/>
            public void Push([NotNull] BaseItem baseItem)
            {
                if (baseItem.GetType() != ItemType)
                    throw new ArgumentException("wrong item type passed");
                CleanData();
                int count = Count;
                if (count >= MaxCount)
                    throw new IndexOutOfRangeException("exceeded max count");

                registry.AddItem(BaseItem.GetBaseItemHashCode(ItemType), new NetworkBehaviourReference(baseItem));
            }

            /// <summary>
            ///     pushes value to the stack
            /// </summary>
            /// <param name="baseItem">the item to push</param>
            /// <returns>true if push succedeed </returns>
            /// <exception cref="ArgumentException">if item type is invalid</exception>
            /// <exception cref="InvalidOperationException">if the caller is not an allowed writer</exception>
            /// <seealso cref="Push"/>
            public bool TryPush([NotNull] BaseItem baseItem)
            {
                try
                {
                    Push(baseItem);
                }
                catch (IndexOutOfRangeException)
                {
                    return false;
                }

                return true;
            }

            public bool CanPush => Count < MaxCount;

            [Pure]
            public List<BaseItem> Data()
            {
                if (!ContainerExists)
                    return new List<BaseItem>();
                return items
                    .Where(v => v.TryGet(out BaseItem item) && item.State == ItemState.Clean)
                    .Select(v =>
                    {
                        v.TryGet(out BaseItem item);
                        return item;
                    }).ToList();
            }

            public IEnumerator<BaseItem> GetEnumerator()
            {
                return Data().GetEnumerator();
            }


            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }

        public struct ItemRegistryEvent
        {
            public enum EventType : byte
            {
                Push,
                Pop,

                /// <summary>
                ///  clears the stack at provided index
                /// </summary>
                RemoveAt,

                /// <summary>
                ///  Fully re-fills the container
                /// </summary>
                Full,

                /// <summary>
                ///     clears the data
                /// </summary>
                Clear
            }

            public EventType Type;

            public int index;

            public long ObjType;

            public NetworkBehaviourReference itemRef;
        }
    }
}