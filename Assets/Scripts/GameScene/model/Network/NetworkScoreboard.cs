using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;

namespace GameScene.model.Network
{
    using PlayerInfo = Dictionary<PlayerInfoField, int>;

    public enum PlayerInfoField : byte
    {
        Kill,
        DamagesDone,
        DamagesReceived,
        MonstersKilled,
        Deaths,
        HealingReceived,
        SpawnedMinions,
    }

    public class NetworkScoreboard : NetworkVariableBase, IReadOnlyDictionary<ulong, PlayerInfo>
    {
        private readonly Dictionary<ulong, PlayerInfo> data = new Dictionary<ulong, PlayerInfo>();
        private readonly List<ScoreboardEvent> dirtyEvents = new List<ScoreboardEvent>();
        
        public bool ContainsKey(ulong key)
        {
            return data.ContainsKey(key);
        }

        public bool TryGetValue(ulong key, out PlayerInfo value)
        {
            return data.TryGetValue(key, out value);
        }

        public PlayerInfo this[ulong playerID] =>
            data.ContainsKey(playerID) ? data[playerID] : default;

        public IEnumerable<ulong> Keys => data.Keys;
        public IEnumerable<PlayerInfo> Values => data.Values;

        public delegate void OnValueChangeDelegate(ScoreboardEvent @event);

        public OnValueChangeDelegate OnValueChanged;

        public NetworkScoreboard(NetworkVariableReadPermission readPermission = DefaultReadPerm,
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
                writer.WriteValueSafe(ScoreboardEvent.EventType.Full);
                WriteField(writer);
                return;
            }

            writer.WriteValueSafe((ushort) dirtyEvents.Count);

            foreach (var @event in dirtyEvents)
            {
                writer.WriteValueSafe(@event.Type);
                switch (@event.Type)
                {
                    case ScoreboardEvent.EventType.Modify:
                        writer.WriteValueSafe(@event.PlayerID);
                        writer.WriteValueSafe(@event.Field);
                        writer.WriteValueSafe(@event.Value);
                        break;
                }
            }
        }

        public override void WriteField(FastBufferWriter writer)
        {
            writer.WriteValueSafe((ushort) data.Count);
            foreach (var pair in data)
            {
                // sends the client id
                writer.WriteValueSafe(pair.Key);

                writer.WriteValueSafe((ushort) pair.Value.Count);
                foreach (var field in pair.Value)
                {
                    // writes field/value
                    writer.WriteValueSafe(field.Key);
                    writer.WriteValueSafe(field.Value);
                }
            }
        }

        public override void ReadField(FastBufferReader reader)
        {
            data.Clear();
            reader.ReadValueSafe(out ushort playerCount);
            for (int i = 0; i < playerCount; i++)
            {
                reader.ReadValueSafe(out ulong clientID);
                data.Add(clientID, new PlayerInfo());

                reader.ReadValueSafe(out ushort count);
                for (int j = 0; j < count; j++)
                {
                    reader.ReadValueSafe(out PlayerInfoField field);
                    reader.ReadValueSafe(out int value);
                    data[clientID][field] = value;
                }
            }

            OnValueChanged?.Invoke(new ScoreboardEvent()
            {
                Type = ScoreboardEvent.EventType.Full
            });
        }

        public override void ReadDelta(FastBufferReader reader, bool keepDirtyDelta)
        {
            reader.ReadValueSafe(out ushort deltaCount);
            for (; deltaCount > 0; deltaCount--)
            {
                reader.ReadValueSafe(out ScoreboardEvent.EventType eventType);
                switch (eventType)
                {
                    case ScoreboardEvent.EventType.Modify:
                    {
                        reader.ReadValueSafe(out ulong playerID);
                        reader.ReadValueSafe(out PlayerInfoField field);
                        reader.ReadValueSafe(out int value);
                        if (!data.ContainsKey(playerID))
                        {
                            data.Add(playerID, new PlayerInfo());
                        }

                        data[playerID][field] = value;
                        var @event = new ScoreboardEvent()
                        {
                            Field = field,
                            Type = eventType,
                            Value = value,
                            PlayerID = playerID
                        };
                        if (keepDirtyDelta)
                            dirtyEvents.Add(@event);

                        OnValueChanged?.Invoke(@event);
                    }
                        break;
                    case ScoreboardEvent.EventType.Clear:
                        data.Clear();
                        if (keepDirtyDelta)
                        {
                            dirtyEvents.Add(new ScoreboardEvent()
                            {
                                Type = eventType,
                            });
                        }

                        break;
                    case ScoreboardEvent.EventType.Full:
                    {
                        ReadField(reader);
                        ResetDirty();
                    }
                        break;
                }
            }
        }

        /// <summary>
        ///     updates in network the scoreboard
        /// </summary>
        /// <param name="playerID"></param>
        /// <param name="field">the field to update</param>
        /// <param name="value"></param>
        /// <param name="delta">whether the <see cref="value"/> is a delta or the final value of the variable</param>
        /// <exception cref="InvalidOperationException">if the client is not allowed to write to the scoreboard</exception>
        public void UpdateData(ulong playerID, PlayerInfoField field, int value, bool delta = false)
        {
            if (!CanClientWrite(NetworkManager.Singleton.LocalClientId))
                throw new InvalidOperationException("this client is not allowed to write to the score board");

            if (!ContainsKey(playerID))
            {
                data[playerID] = new PlayerInfo();
            }
            else if (data[playerID].ContainsKey(field) && delta)
            {
                value += data[playerID][field];
            }

            data[playerID][field] = value;

            dirtyEvents.Add(new ScoreboardEvent()
            {
                Type = ScoreboardEvent.EventType.Modify,
                PlayerID = playerID,
                Field = field,
                Value = value
            });
        }

        /// <summary>
        ///     clears the scoreboard
        /// </summary>
        /// <exception cref="InvalidOperationException">if the client is not allowed to write to the scoreboard</exception>
        public void Clear()
        {
            if (!CanClientWrite(NetworkManager.Singleton.LocalClientId))
                throw new InvalidOperationException("this client is not allowed to write to the score board");

            data.Clear();
            dirtyEvents.Add(new ScoreboardEvent()
            {
                Type = ScoreboardEvent.EventType.Clear
            });
        }

        public struct ScoreboardEvent
        {
            public enum EventType : byte
            {
                Full,
                Modify,
                Clear
            }

            public EventType Type;
            public ulong PlayerID;

            // the field modified
            public PlayerInfoField Field;

            // the new value of the field
            public int Value;
        }

        public IEnumerator<KeyValuePair<ulong, PlayerInfo>> GetEnumerator()
        {
            return data.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public int Count => data.Count;
    }
}