﻿using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;

namespace model
{
    public class NetworkPlayersRegistry<TNSerializable> : NetworkVariableBase, IDictionary<ulong, TNSerializable>
        where TNSerializable : INetworkSerializable, new()
    {
        private ulong LocalClientId => NetworkManager.Singleton.LocalClientId;

        private readonly Dictionary<ulong, TNSerializable> data = new Dictionary<ulong, TNSerializable>();
        private readonly List<PlayersRegistryEvent> dirtyEvents = new List<PlayersRegistryEvent>();

        public delegate void OnValueChangedDelegate(PlayersRegistryEvent @event);

        public OnValueChangedDelegate OnValueChanged = null;

        public NetworkPlayersRegistry(NetworkVariableReadPermission readPermission = DefaultReadPerm,
            NetworkVariableWritePermission writePermission = DefaultWritePerm) : base(readPermission, writePermission)
        {
        }

        public override bool IsDirty()
        {
            return base.IsDirty() || dirtyEvents.Count > 0;
        }

        public override void ResetDirty()
        {
            base.ResetDirty();
            dirtyEvents.Clear();
        }

        public override void WriteDelta(FastBufferWriter writer)
        {
            if (base.IsDirty())
            {
                writer.WriteValue((ushort) 1);
                writer.WriteValueSafe(PlayersRegistryEvent.EventType.Full);
                WriteField(writer);
                return;
            }

            writer.WriteValueSafe((ushort) dirtyEvents.Count);
            foreach (var dirtyEvent in dirtyEvents)
            {
                writer.WriteValueSafe(dirtyEvent.Type);
                switch (dirtyEvent.Type)
                {
                    case PlayersRegistryEvent.EventType.Add:
                        writer.WriteValueSafe(dirtyEvent.key);
                        writer.WriteNetworkSerializable(dirtyEvent.Data);
                        break;
                    case PlayersRegistryEvent.EventType.Remove:
                        writer.WriteValueSafe(dirtyEvent.key);
                        break;
                }
            }
        }

        public override void WriteField(FastBufferWriter writer)
        {
            writer.WriteValueSafe((ushort) data.Count);
            foreach (var pair in data)
            {
                writer.WriteValueSafe(pair.Key);
                writer.WriteNetworkSerializable(pair.Value);
            }
        }

        public override void ReadField(FastBufferReader reader)
        {
            data.Clear();
            reader.ReadValueSafe(out ushort count);
            for (int i = 0; i < count; i++)
            {
                reader.ReadValueSafe(out ulong key);
                reader.ReadNetworkSerializable(out TNSerializable serializable);
                data[key] = serializable;
            }

            OnValueChanged?.Invoke(new PlayersRegistryEvent()
            {
                Type = PlayersRegistryEvent.EventType.Full
            });
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

        public struct PlayersRegistryEvent
        {
            public enum EventType : byte
            {
                Full,
                Add,
                Remove,
                Clear
            }

            public EventType Type;
            public ulong key;
            public TNSerializable Data;
        }
    }
}