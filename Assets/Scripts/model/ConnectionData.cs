using Unity.Netcode;

namespace model
{
    public class ConnectionData : INetworkSerializable
    {
        /// <summary>
        ///     the pseudo of the local player
        /// </summary>
        public string Pseudo = null;

        /// <summary>
        ///     the name of the room to connect to
        /// </summary>
        public string RoomName = null;

        /**
         * <value>the name of the class chosen by the player</value>
         */
        public string ClassName = null;

        /**
         * <value>the type of session the <see cref="Unity.Netcode.NetworkManager"/> should start</value>
         */
        public string ConnectionType = null;

        /// <summary>
        ///     the number of players required for the game to begin (can be 4 or 6)
        /// </summary>
        /// <remarks>only the server can provide this information</remarks>
        public int PlayerAmount = 4;

        /**
         * <value>the team of the logged in player</value>
         */
        public int TeamId = -1;

        public bool IsReady => !(ClassName == null || ConnectionType == null || TeamId < 0);

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref Pseudo);
            serializer.SerializeValue(ref RoomName);
            serializer.SerializeValue(ref ClassName);
            serializer.SerializeValue(ref ConnectionType);
            serializer.SerializeValue(ref TeamId);
            serializer.SerializeValue(ref PlayerAmount);
        }
    }
}