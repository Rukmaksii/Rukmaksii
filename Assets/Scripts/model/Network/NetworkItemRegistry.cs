using System;
using System.Collections.Generic;
using System.Linq;
using Items;
using JetBrains.Annotations;
using Unity.Netcode;

namespace model.Network
{
    public class NetworkItemRegistry : NetworkVariableBase
    {
        private List<ItemRegistryEvent> dirtyEvents = new List<ItemRegistryEvent>();

        // binds item type hash code to list of item references
        private Dictionary<long, List<NetworkBehaviourReference>> data =
            new Dictionary<long, List<NetworkBehaviourReference>>();

        public ItemContainer this[Type itemType] => new ItemContainer(BaseItem.ItemInfos[itemType].MaxCount, itemType, this);


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
                        data[objType].RemoveAt(index);

                        if (data[objType].Count <= 0)
                            data.Remove(objType);

                        if (keepDirtyDelta)
                        {
                            dirtyEvents.Add(new ItemRegistryEvent()
                            {
                                Type = eventType,
                                ObjType = objType,
                                index = index
                            });
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
                        if (keepDirtyDelta)
                        {
                            dirtyEvents.Add(new ItemRegistryEvent()
                            {
                                Type = eventType,
                                ObjType = objType,
                                itemRef = itemRef
                            });
                        }
                    }
                        break;
                    case ItemRegistryEvent.EventType.Pop:
                    {
                        reader.ReadValueSafe(out long objType);
                        data[objType].RemoveAt(data[objType].Count - 1);

                        if (data[objType].Count <= 0)
                            data.Remove(objType);

                        if (keepDirtyDelta)
                        {
                            dirtyEvents.Add(new ItemRegistryEvent()
                            {
                                Type = eventType,
                                ObjType = objType
                            });
                        }
                    }
                        break;
                    case ItemRegistryEvent.EventType.Clear:
                        data.Clear();
                        if (keepDirtyDelta)
                        {
                            dirtyEvents.Add(new ItemRegistryEvent()
                            {
                                Type = eventType
                            });
                        }

                        break;
                    case ItemRegistryEvent.EventType.Full:
                        ReadField(reader);
                        ResetDirty();
                        break;
                }
            }
        }


        private void RemoveAt(long objectType, int index)
        {
            data[objectType].RemoveAt(index);
            if (data[objectType].Count <= 0)
                data.Remove(objectType);
            dirtyEvents.Add(new ItemRegistryEvent()
            {
                Type = ItemRegistryEvent.EventType.RemoveAt,
                ObjType = objectType,
                index = index
            });
        }

        private void AddItem(long objectType, NetworkBehaviourReference itemRef)
        {
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

        public class ItemContainer
        {
            public readonly int MaxCount;
            public readonly Type ItemType;
            private readonly NetworkItemRegistry registry;
            private List<NetworkBehaviourReference> items => registry.data[BaseItem.GetBaseItemHashCode(ItemType)];
            private bool ContainerExists => registry.ContainsKey(ItemType);

            public int Count
            {
                get
                {
                    if (!ContainerExists)
                        return -1;
                    CleanData();
                    return items.Count;
                }
            }

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
            /// <exception cref="InvalidOperationException">if there is no item to pop</exception>
            /// <seealso cref="TryPop"/>
            public BaseItem Pop()
            {
                // cleans data
                int count = Count;
                if (count <= 0)
                    throw new InvalidOperationException("could not pop from an empty stack");

                items[count - 1].TryGet(out BaseItem result);
                registry.RemoveAt(BaseItem.GetBaseItemHashCode(ItemType), Count - 1);

                return result;
            }

            /// <summary>
            ///     peeks the top of the stack
            /// </summary>
            /// <returns>the peaked base item</returns>
            /// <remarks>peeking does not modify the stack top</remarks>
            /// <exception cref="InvalidOperationException">if the stack is empty</exception>
            /// <seealso cref="TryPeek"/>
            public BaseItem Peek()
            {
                // cleans data
                int count = Count;
                if (count <= 0)
                    throw new InvalidOperationException("could not peek from an empty stack");

                items[count - 1].TryGet(out BaseItem result);
                return result;
            }

            /// <summary>
            ///     tries peeking the top of the stack
            /// </summary>
            /// <param name="result">the peeked to</param>
            /// <returns>true if the stack was not empty</returns>
            /// <remarks>peeking does not modify the stack top</remarks>
            /// <seealso cref="Peek"/>
            public bool TryPeek(out BaseItem result)
            {
                try
                {
                    result = Peek();
                }
                catch (InvalidOperationException)
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
            public bool TryPop(out BaseItem result)
            {
                try
                {
                    result = Pop();
                }
                catch (InvalidOperationException)
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
            /// <seealso cref="TryPush"/>
            public void Push([NotNull] BaseItem baseItem)
            {
                if (baseItem.GetType() != ItemType)
                    throw new ArgumentException("wrong item type passed");
                // cleans data
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

            public IEnumerator<BaseItem> GetEnumerator()
            {
                if (!ContainerExists)
                    return new List<BaseItem>().GetEnumerator();
                CleanData();
                return items
                    .Where(v => v.TryGet(out BaseItem _))
                    .Select(v =>
                    {
                        v.TryGet(out BaseItem item);
                        return item;
                    }).GetEnumerator();
            }
        }

        struct ItemRegistryEvent
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