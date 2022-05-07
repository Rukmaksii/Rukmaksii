using System;
using System.Collections.Generic;
using Unity.Netcode;

namespace model.Network
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
    }

    public class NetworkScoreboard : NetworkVariableBase
    {
        private Dictionary<ulong, PlayerInfo> data = new Dictionary<ulong, PlayerInfo>();
        private List<ScoreboardEvent> dirtyEvents = new List<ScoreboardEvent>();

        public PlayerInfo this[ulong playerID] =>
            data.ContainsKey(playerID) ? data[playerID] : default;

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
        }

        public override void ReadDelta(FastBufferReader reader, bool keepDirtyDelta)
        {
            throw new NotImplementedException();
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
    }
}