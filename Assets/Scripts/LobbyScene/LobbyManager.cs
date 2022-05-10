using GameManagers;
using model;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LobbyManager : NetworkBehaviour
{
    [SerializeField] private ConnectionManagerScript connectionManager;

    [SerializeField] private ConnectionScriptableObject connectionData;
    [SerializeField] private GameObject playerViewer;
    [SerializeField] private Button startButton;

    private NetworkVariable<int> playerCount = new NetworkVariable<int>();

    public readonly NetworkPlayersRegistry<ConnectionData> PlayersRegistry =
        new NetworkPlayersRegistry<ConnectionData>();

    public int PlayerCount => playerCount.Value;

    public bool CanStart => PlayersRegistry.Count > PlayerCount;

    // Start is called before the first frame update
    void Start()
    {
        NetworkManager.Singleton.OnClientConnectedCallback += delegate(ulong clientId)
        {
            if (!NetworkManager.Singleton.IsClient)
                return;
            AddPlayerServerRpc(clientId, connectionData.Data);
        };

        DontDestroyOnLoad(gameObject);

        if (IsServer)
        {
            playerCount.Value = connectionData.Data.PlayerAmount;
            NetworkManager.Singleton.ConnectionApprovalCallback +=
                delegate(byte[] data, ulong clientId, NetworkManager.ConnectionApprovedDelegate cb)
                {
                    bool createPlayer = true;
                    bool approve = PlayersRegistry.Count < PlayerCount;
                    cb(createPlayer, null, approve, Vector3.zero, Quaternion.identity);
                };
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (IsServer)
        {
            startButton.gameObject.SetActive(true);
            startButton.interactable = startButton.enabled = CanStart;
        }
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