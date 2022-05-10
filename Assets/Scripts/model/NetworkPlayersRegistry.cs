using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
                        writer.WriteValueSafe(dirtyEvent.Key);
                        writer.WriteNetworkSerializable(dirtyEvent.Data);
                        break;
                    case PlayersRegistryEvent.EventType.Remove:
                        writer.WriteValueSafe(dirtyEvent.Key);
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
            reader.ReadValueSafe(out ushort deltaCount);
            for (int i = 0; i < deltaCount; i++)
            {
                reader.ReadValueSafe(out PlayersRegistryEvent.EventType eventType);
                switch (eventType)
                {
                    case PlayersRegistryEvent.EventType.Add:
                    {
                        reader.ReadValueSafe(out ulong key);
                        reader.ReadNetworkSerializable(out TNSerializable value);
                        data[key] = value;

                        var ev = new PlayersRegistryEvent()
                        {
                            Type = eventType,
                            Key = key,
                            Data = value
                        };

                        if (keepDirtyDelta)
                            dirtyEvents.Add(ev);
                        OnValueChanged?.Invoke(ev);
                    }
                        break;
                    case PlayersRegistryEvent.EventType.Remove:
                    {
                        reader.ReadValueSafe(out ulong key);
                        data.Remove(key);

                        var ev = new PlayersRegistryEvent()
                        {
                            Type = eventType,
                            Key = key
                        };

                        if (keepDirtyDelta)
                            dirtyEvents.Add(ev);
                        OnValueChanged?.Invoke(ev);
                    }
                        break;
                    case PlayersRegistryEvent.EventType.Clear:
                    {
                        data.Clear();

                        var ev = new PlayersRegistryEvent()
                        {
                            Type = eventType,
                        };

                        if (keepDirtyDelta)
                            dirtyEvents.Add(ev);
                        OnValueChanged?.Invoke(ev);
                    }
                        break;
                    case PlayersRegistryEvent.EventType.Full:
                        ReadField(reader);
                        ResetDirty();
                        break;
                }
            }
        }

        public IEnumerator<KeyValuePair<ulong, TNSerializable>> GetEnumerator() => data.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public void Add(KeyValuePair<ulong, TNSerializable> item)
            => Add(item.Key, item.Value);


        public void Clear()
        {
            if (IsReadOnly)
                throw new InvalidOperationException("Client cannot write to player registry");
            data.Clear();
            dirtyEvents.Add(new PlayersRegistryEvent()
            {
                Type = PlayersRegistryEvent.EventType.Clear
            });
        }

        public bool Contains(KeyValuePair<ulong, TNSerializable> item)
            => data.Contains(item);


        public void CopyTo(KeyValuePair<ulong, TNSerializable>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public bool Remove(KeyValuePair<ulong, TNSerializable> item)
            => Remove(item.Key);

        public int Count => data.Count;
        public bool IsReadOnly => !CanClientWrite(LocalClientId);

        public void Add(ulong key, TNSerializable value)
        {
            if (IsReadOnly)
                throw new InvalidOperationException("Client cannot write to player registry");

            data.Add(key, value);
            dirtyEvents.Add(new PlayersRegistryEvent()
            {
                Type = PlayersRegistryEvent.EventType.Add,
                Key = key,
                Data = value
            });
        }

        public bool ContainsKey(ulong key)
            => data.ContainsKey(key);

        public bool Remove(ulong key)
        {
            if (IsReadOnly)
                throw new InvalidOperationException("Client cannot write to players registry");

            if (!ContainsKey(key) || !data.Remove(key))
                return false;
            dirtyEvents.Add(new PlayersRegistryEvent()
            {
                Type = PlayersRegistryEvent.EventType.Remove,
                Key = key
            });

            return true;
        }

        public bool TryGetValue(ulong key, out TNSerializable value)
        {
            if (!ContainsKey(key))
            {
                value = default;
                return false;
            }

            value = this[key];
            return true;
        }

        public TNSerializable this[ulong key]
        {
            get => data[key];
            set => Add(key, value);
        }

        public ICollection<ulong> Keys => data.Keys;
        public ICollection<TNSerializable> Values => data.Values;

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
            public ulong Key;
            public TNSerializable Data;
        }
    }
}