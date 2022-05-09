namespace model
{


    public class ConnectionData
    {
        /**
         * <value>the name of the class chosen by the player</value>
         */
        public string ClassName = null;

        /**
         * <value>the type of session the <see cref="Unity.Netcode.NetworkManager"/> should start</value>
         */
        public string ConnectionType = null;

        /**
         * <value>the team of the logged in player</value>
         */
        public int TeamId = -1;

        public bool IsReady => !(ClassName == null || ConnectionType == null || TeamId < 0);

    }
}