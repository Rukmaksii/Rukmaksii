using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace GameManagers
{
    public class ConnectionManagerScript : MonoBehaviour
    {
        [SerializeField] private List<GameObject> classPrefabs = new List<GameObject>();

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