using System.Collections.Generic;
using model;
using PlayerControllers;
using Unity.Netcode;
using UnityEngine;

namespace GameManagers
{
    public class ConnectionManagerScript : MonoBehaviour
    {
        [SerializeField] private ConnectionScriptableObject connectionData;
        [SerializeField] private List<GameObject> classPrefabs = new List<GameObject>();


        void Start()
        {

            // kept for local tests
            // TODO : remove this line and OnGui
            if (connectionData.Data == null)
                return;
            
            GameObject playerPrefab = classPrefabs.Find(go =>
                go.GetComponent<BasePlayer>().ClassName == connectionData.Data.ClassName);

            // TODO : class prefabs are not empty
            if (playerPrefab == null)
                playerPrefab = classPrefabs[0];

            NetworkManager.Singleton.NetworkConfig.PlayerPrefab = playerPrefab;

            switch (connectionData.Data.ConnectionType)
            {
                case "host":
                default:
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
            NetworkManager.Singleton.NetworkConfig.PlayerPrefab = classPrefabs[0];
            if (GUILayout.Button("Host"))
            {
                NetworkManager.Singleton.StartHost();
            }
            else if (GUILayout.Button("Server"))
            {
                NetworkManager.Singleton.StartServer();
            }

            else if (GUILayout.Button("Client"))
                NetworkManager.Singleton.StartClient();
        }
    }
}