using System;
using System.Collections.Generic;
using Unity.Netcode;

namespace model.Network
{
    public class NetworkItemRegistry : NetworkVariableBase
    {
        private List<ItemRegistryEvent> dirtyEvents = new List<ItemRegistryEvent>();

        private Dictionary<Type, List<NetworkBehaviourReference>> data =
            new Dictionary<Type, List<NetworkBehaviourReference>>();

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
            throw new NotImplementedException();
        }

        public override void WriteField(FastBufferWriter writer)
        {
            throw new NotImplementedException();
        }

        public override void ReadField(FastBufferReader reader)
        {
            throw new NotImplementedException();
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
                Clear
            }

            public int index;

            public Type ItemType;

            public NetworkBehaviourReference itemRef;
        }
    }
}