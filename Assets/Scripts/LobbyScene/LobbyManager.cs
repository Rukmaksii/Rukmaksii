using GameManagers;
using model;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LobbyManager : NetworkBehaviour
{
    [SerializeField] private ConnectionManagerScript connectionManager;

    [SerializeField] private ConnectionScriptableObject connectionData;
    [SerializeField] private GameObject playerViewer;

    private NetworkVariable<int> playerCount = new NetworkVariable<int>();

    public readonly NetworkPlayersRegistry<ConnectionData> PlayersRegistry =
        new NetworkPlayersRegistry<ConnectionData>();

    public int PlayerCount => playerCount.Value;

    // Start is called before the first frame update
    void Start()
    {
        if (IsServer)
        {
            playerCount.Value = connectionData.Data.PlayerAmount;
        }

        NetworkManager.Singleton.OnClientConnectedCallback += delegate(ulong clientId)
        {
            if (!NetworkManager.Singleton.IsClient)
                return;
            AddPlayerServerRpc(clientId, connectionData.Data);
        };
    }

    // Update is called once per frame
    void Update()
    {
    }

    [ServerRpc(RequireOwnership = false)]
    private void AddPlayerServerRpc(ulong playerId, ConnectionData data)
    {
        PlayersRegistry[playerId] = data;
    }

    [ServerRpc(RequireOwnership = false)]
    private void RemovePlayerServerRpc(ulong playerId)
    {
        PlayersRegistry.Remove(playerId);
    }

    public void StartGame()
    {
        NetworkManager.Singleton.SceneManager.LoadScene("GameScene", LoadSceneMode.Single);
    }
}