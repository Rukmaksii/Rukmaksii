using Unity.Netcode;
using Unity.Netcode.Transports.UNET;
using UnityEngine;

namespace GameManagers
{
    public class ConnectionManagerScript : MonoBehaviour
    {
        [SerializeField] private ConnectionScriptableObject connectionData;


        public Gameloop gameloop;

        void Start()
        {
#if UNET
            NetworkManager.Singleton.NetworkConfig.NetworkTransport = GetComponent<UNetTransport>();
#endif

            gameloop = gameObject.AddComponent<Gameloop>();
            // kept for local tests
            // TODO : remove this line and OnGui
            if (!connectionData.Data.IsReady)
                return;

            // NetworkManager.Singleton.NetworkConfig.PlayerPrefab = playerPrefab;

            switch (connectionData.Data.ConnectionType)
            {
                case "host":
                    NetworkManager.Singleton.StartHost();
                    break;
                case "client":
                    NetworkManager.Singleton.StartClient();
                    break;
                case "server":
                    NetworkManager.Singleton.StartServer();
                    break;
            }
        }

        private void OnGUI()
        {
            GUILayout.BeginArea(new Rect(10, 10, 300, 300));
            if (!NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer)
            {
                StartButtons();
            }

            GUILayout.EndArea();
        }

        void StartButtons()
        {
            // NetworkManager.Singleton.NetworkConfig.PlayerPrefab = classPrefabs[0];
            if (GUILayout.Button("Host"))
            {
                NetworkManager.Singleton.StartHost();
            }
            else if (GUILayout.Button("Server"))
            {
                NetworkManager.Singleton.StartServer();
            }

            else if (GUILayout.Button("Client"))
            {
                NetworkManager.Singleton.StartClient();
            }
        }
    }
}