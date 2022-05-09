using System;
using model;
using PlayerControllers;
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
#if DEBUG
            NetworkManager.Singleton.NetworkConfig.NetworkTransport = GetComponent<UNetTransport>();
#endif
        }

        private void Update()
        {
            if (!NetworkManager.Singleton.IsServer && !NetworkManager.Singleton.IsClient)
            {
                if (connectionData.Data.IsReady)
                {
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
            }
        }

#if DEBUG
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
                connectionData.Data.ConnectionType = "host";
            }
            else if (GUILayout.Button("Server"))
            {
                connectionData.Data.ConnectionType = "server";
            }

            else if (GUILayout.Button("Client"))
            {
                connectionData.Data.ConnectionType = "client";
            }
            else
            {
                return;
            }

            connectionData.Data.ClassName = "test class";
            connectionData.Data.TeamId = 0;

            GameController.Singleton.PlayerUIInstance.SetActive(true);
        }
    }

#endif
}