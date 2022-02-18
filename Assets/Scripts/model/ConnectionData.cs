namespace model
{


    public class ConnectionData
    {
        /**
         * <value>the name of the class chosen by the player</value>
         */
        public string ClassName = "";

        /**
         * <value>the type of session the <see cref="Unity.Netcode.NetworkManager"/> should start</value>
         */
        public string ConnectionType = "host";

        /**
         * <value>the team of the logged in player</value>
         */
        public int TeamId = 0;

    }
}