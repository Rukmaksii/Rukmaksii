using System.Collections.Generic;
using System.Collections;
using model;
using PlayerControllers;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using UnityEngine.AI;

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
                    StartCoroutine(waitagent());
                    break;
                case "client":
                    NetworkManager.Singleton.StartClient();
                    break;
                case "server":
                    NetworkManager.Singleton.StartServer();
                    StartCoroutine(waitagent());
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
                StartCoroutine(waitagent());
            }
            else if (GUILayout.Button("Server"))
            {
                NetworkManager.Singleton.StartServer();
                StartCoroutine(waitagent());
            }

            else if (GUILayout.Button("Client"))
                NetworkManager.Singleton.StartClient();
        }
        
        IEnumerator waitagent()
        {
            yield return new WaitForSeconds(10);
            for (int i = 0; i < 4; i++)
            {
                GameObject instance = Instantiate(GameController.Singleton.MonsterPrefab);
                instance.GetComponent<NetworkObject>().Spawn();
                instance.gameObject.AddComponent<NavMeshAgent>();
            }
        }
    }
}