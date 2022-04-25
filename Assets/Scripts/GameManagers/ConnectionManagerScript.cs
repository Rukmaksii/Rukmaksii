using Unity.Netcode;
using Unity.Netcode.Transports.UNET;
using UnityEngine;

namespace GameManagers
{
    public class ConnectionManagerScript : MonoBehaviour
    {
        [SerializeField] private ConnectionScriptableObject connectionData;


        void Start()
        {
#if UNET
            NetworkManager.Singleton.NetworkConfig.NetworkTransport = GetComponent<UNetTransport>();
#endif


            // kept for local tests
            // TODO : remove this line and OnGui
            if (!connectionData.Data.IsReady)
                return;


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

            //GameController.Singleton.PlayerUIInstance.SetActive(true);
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
            else
            {
                return;
            }
            GameController.Singleton.PlayerUIInstance.SetActive(true);
        }
    }
}