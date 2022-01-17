using Unity.Netcode;
using UnityEngine;

namespace GameManagers
{
    public class ConnectionManagerScript : MonoBehaviour
    {
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
                NetworkManager.Singleton.ConnectionApprovalCallback += ApprovalCheck;
                NetworkManager.Singleton.StartHost();
            }
            else if (GUILayout.Button("Server"))
            {
                NetworkManager.Singleton.ConnectionApprovalCallback += ApprovalCheck;
                NetworkManager.Singleton.StartServer();
            }

            else if (GUILayout.Button("Client"))
                NetworkManager.Singleton.StartClient();
        }

        private void ApprovalCheck(byte[] connectionData, ulong clientId,
            NetworkManager.ConnectionApprovedDelegate callback)
        {
            bool approve = true, createPlayerObject = true;
            
            Debug.Log("check connection approval");


            uint? playerPrefabHash = null;

            Vector3? positionToSpawnAt = Vector3.zero;
            Quaternion rotation = Quaternion.identity;

            callback(createPlayerObject, playerPrefabHash, approve, positionToSpawnAt, rotation);
        }
    }
}