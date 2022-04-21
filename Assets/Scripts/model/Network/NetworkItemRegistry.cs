using System;
using System.Collections.Generic;
using Unity.Netcode;

namespace model.Network
{
    public class NetworkItemRegistry : NetworkVariableBase
    {
        private List<ItemRegistryEvent> dirtyEvents = new List<ItemRegistryEvent>();

        // binds item type hash code to list of item references
        private Dictionary<long, List<NetworkBehaviourReference>> data =
            new Dictionary<long, List<NetworkBehaviourReference>>();

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
                    case ItemRegistryEvent.EventType.Clear:
                        // remove at
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
            reader.ReadValue(out ushort containerCount);
            for (int i = 0; i < containerCount; i++)
            {
                // fetches type hashcode
                reader.ReadValue(out long objType);
                // fetches item count
                reader.ReadValue(out int itemCount);
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
            throw new NotImplementedException();
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
                Clear,

                /// <summary>
                ///  Fully re-fills the container
                /// </summary>
                Full
            }

            public EventType Type;

            public int index;

            public long ObjType;

            public NetworkBehaviourReference itemRef;
        }
    }
}