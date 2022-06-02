// ReSharper disable once RedundantUsingDirective

using System;
using GameScene.model.Network;
using Netcode.Transports.PhotonRealtime;
using Unity.Netcode;
using Unity.Netcode.Transports.UNET;
using UnityEngine;

namespace model
{
    public class ConnectionManagerScript : MonoBehaviour
    {
        [SerializeField] private ConnectionScriptableObject connectionData;

        public static ConnectionManagerScript Singleton { get; private set; }

        private void Awake()
        {
            if (Singleton != null && Singleton != this)
            {
                Destroy(this.gameObject);
                return;
            }

            Singleton = this;
        }

        void Start()
        {
            NetworkManager.Singleton.NetworkConfig.ConnectionApproval = true;
#if UNET
            if (NetworkManager.Singleton.IsClient || NetworkManager.Singleton.IsHost)
            {
                gameObject.SetActive(false);
                return;
            }

            NetworkManager.Singleton.NetworkConfig.NetworkTransport =
                GetComponent<UNetTransport>();
#else
            if (DebugManager.IsDebug)
            {
                var transport = GetComponent<UNetTransport>();
                NetworkManager.Singleton.NetworkConfig.NetworkTransport = transport;
                if (!String.IsNullOrEmpty(connectionData.Data.RoomName))
                {
                    var values = connectionData.Data.RoomName.Split(':');
                    if (values.Length > 0)
                    {
                        transport.ConnectAddress = values[0];
                        if (values.Length > 1 && int.TryParse(values[1], out int port))
                        {
                            transport.ConnectPort = port;
                            transport.ServerListenPort = port;
                        }
                    }
                }
            }
            else
            {
                NetworkManager.Singleton.NetworkConfig.NetworkTransport =
                    GetComponent<PhotonRealtimeTransport>();
                GetComponent<PhotonRealtimeTransport>().RoomName = connectionData.Data.RoomName;
            }

#endif
        }


        private void Update()
        {
            if (!NetworkManager.Singleton.IsServer && !NetworkManager.Singleton.IsClient)
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

#if UNET
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

            // connectionData.Data.ClassName = "test class";
            connectionData.Data.TeamId = 0;
        }
#endif

        
    }
}