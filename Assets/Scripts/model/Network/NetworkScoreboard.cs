using System;
using System.Collections.Generic;
using Unity.Netcode;

namespace model.Network
{
    public struct PlayerInfos
    {
        public int Kills;
        public int DamagesDone;
        public int DamagesReceived;
        public int MonstersKilled;
        public int Deaths;
        public int HealingReceived;
    }

    public class NetworkScoreboard : NetworkVariableBase
    {
        private Dictionary<ulong, PlayerInfos> data = new Dictionary<ulong, PlayerInfos>();

        public delegate void OnValueChangeDelegate(ScoreboardEvent @event);

        public OnValueChangeDelegate OnValueChanged;

        public NetworkScoreboard(NetworkVariableReadPermission readPermission = DefaultReadPerm,
            NetworkVariableWritePermission writePermission = DefaultWritePerm) : base(readPermission, writePermission)
        {
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
            public int Key;

            // the new value of the field
            public int Value;
        }
    }
}