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

        /// <summary>the team of the logged in player</summary>
        /// <remarks>id is 0 based</remarks>
        public int TeamId = -1;

        /// <summary>
        ///     the id of the player in the team 
        /// </summary>
        /// <remarks>id is 0 based</remarks>
        public int PlayerTeamId = -1;


        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            if (serializer.IsReader)
            {
                serializer.SerializeValue(ref ClassName);
                if (ClassName.Length <= 0)
                    ClassName = null;
                
                serializer.SerializeValue(ref Pseudo);
                if (Pseudo.Length <= 0)
                    Pseudo = null;
                
                serializer.SerializeValue(ref RoomName);
                if (RoomName.Length <= 0)
                    RoomName = null;
            }
            else
            {
                if (ClassName == null)
                {
                    var data = "";
                    serializer.SerializeValue(ref data);
                }
                else
                {
                    serializer.SerializeValue(ref ClassName);
                }

                if (Pseudo == null)
                {
                    var data = "";
                    serializer.SerializeValue(ref data);
                }
                else
                {
                    serializer.SerializeValue(ref Pseudo);
                }

                if (RoomName == null)
                {
                    var data = "";
                    serializer.SerializeValue(ref data);
                }
                else
                {
                    serializer.SerializeValue(ref RoomName);
                }
            }

            serializer.SerializeValue(ref ConnectionType);
            serializer.SerializeValue(ref TeamId);
            serializer.SerializeValue(ref PlayerAmount);
            serializer.SerializeValue(ref PlayerTeamId);
        }
    }
}