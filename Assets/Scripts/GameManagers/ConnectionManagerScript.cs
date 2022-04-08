using System.Collections.Generic;
using System.Collections;
using model;
using PlayerControllers;
using Unity.Netcode;
using Unity.Netcode.Components;
using Unity.Netcode.Transports.UNET;
using UnityEngine;
using UnityEngine.AI;
using System;
using Unity.XR.OpenVR;
using Random = UnityEngine.Random;

namespace GameManagers
{
    public class ConnectionManagerScript : MonoBehaviour
    {
        [SerializeField] private ConnectionScriptableObject connectionData;
        [SerializeField] private List<GameObject> classPrefabs = new List<GameObject>();


        public Gameloop gameloop;
        void Start()
        {
#if UNET
            NetworkManager.Singleton.NetworkConfig.NetworkTransport = GetComponent<UNetTransport>();
#endif
            // kept for local tests
            // TODO : remove this line and OnGui
            if (!connectionData.Data.IsReady)
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
                    NetworkManager.Singleton.StartHost();
                    gameloop = gameObject.AddComponent<Gameloop>();
                    break;
                case "client":
                    NetworkManager.Singleton.StartClient();
                    gameloop = gameObject.AddComponent<Gameloop>();
                    break;
                case "server":
                    NetworkManager.Singleton.StartServer();
                    gameloop = gameObject.AddComponent<Gameloop>();
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
                gameloop = gameObject.AddComponent<Gameloop>();
            }
            else if (GUILayout.Button("Server"))
            {
                NetworkManager.Singleton.StartServer();
                gameloop = gameObject.AddComponent<Gameloop>();
            }

            else if (GUILayout.Button("Client"))
            {
                NetworkManager.Singleton.StartClient();
                gameloop = gameObject.AddComponent<Gameloop>();
            }
        }
    }
}