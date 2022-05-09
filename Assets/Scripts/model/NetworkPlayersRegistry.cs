using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;

namespace model
{
    public class NetworkPlayersRegistry<TNSerializable> : NetworkVariableBase, IDictionary<ulong, TNSerializable>
        where TNSerializable : INetworkSerializable
    {
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

        public IEnumerator<KeyValuePair<ulong, TNSerializable>> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(KeyValuePair<ulong, TNSerializable> item)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public bool Contains(KeyValuePair<ulong, TNSerializable> item)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(KeyValuePair<ulong, TNSerializable>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public bool Remove(KeyValuePair<ulong, TNSerializable> item)
        {
            throw new NotImplementedException();
        }

        public int Count { get; }
        public bool IsReadOnly { get; }

        public void Add(ulong key, TNSerializable value)
        {
            throw new NotImplementedException();
        }

        public bool ContainsKey(ulong key)
        {
            throw new NotImplementedException();
        }

        public bool Remove(ulong key)
        {
            throw new NotImplementedException();
        }

        public bool TryGetValue(ulong key, out TNSerializable value)
        {
            throw new NotImplementedException();
        }

        public TNSerializable this[ulong key]
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }

        public ICollection<ulong> Keys { get; }
        public ICollection<TNSerializable> Values { get; }
    }
}