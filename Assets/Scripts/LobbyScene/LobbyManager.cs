using System.Collections.Generic;
using System.Linq;
using GameManagers;
using model;
using PlayerControllers;
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
    [SerializeField] private Canvas lobbyUI;

    private NetworkVariable<int> playerCount = new NetworkVariable<int>();

    public readonly NetworkPlayersRegistry<ConnectionData> PlayersRegistry =
        new NetworkPlayersRegistry<ConnectionData>();

    public int PlayerCount => playerCount.Value;

    public bool CanStart => PlayersRegistry.Count > PlayerCount;


    public static LobbyManager Singleton { get; private set; }

    private void Awake()
    {
        if (Singleton != null && Singleton != this)
        {
            Destroy(this);
            return;
        }

        Singleton = this;
    }

    private BasePlayer PlayerClass
    {
        get { return null; }
    }

    // Start is called before the first frame update
    void Start()
    {
        PlayersRegistry.OnValueChanged += (@event) => { FillPlayerViewers(); };
        NetworkManager.Singleton.OnClientConnectedCallback += (clientId) =>
        {
            if (!NetworkManager.Singleton.IsClient)
                return;

            if (NetworkManager.Singleton.IsServer)
            {
                PlayersRegistry[clientId] = connectionData.Data;
                FillPlayerViewers();
            }
            else
            {
                AddPlayerServerRpc(clientId, connectionData.Data);
            }
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
        if (!IsSpawned || SceneManager.GetActiveScene().name != "LobbyScene")
            return;
        if (IsServer)
        {
            startButton.gameObject.SetActive(true);
            startButton.interactable = startButton.enabled = CanStart;
        }
    }

    private void FillPlayerViewers()
    {
        var anchors = lobbyUI.GetComponentsInChildren<RectTransform>().Where(t => t.name.StartsWith("Anchor")).ToList();
        anchors.ForEach(a =>
        {
            if (a.childCount == 1)
                Destroy(a.GetChild(0));
        });

        List<RectTransform> t1Anchors = new List<RectTransform>();
        List<RectTransform> t2Anchors = new List<RectTransform>();
        foreach (var a in anchors)
        {
            if (a.parent.name == "Team1")
                t1Anchors.Add(a);
            else
                t2Anchors.Add(a);
        }

        int c1 = 0, c2 = 0;
        foreach (var data in PlayersRegistry.Values)
        {
            Transform parent;
            if (data.TeamId == 0)
            {
                parent = t1Anchors[c1++].transform;
            }
            else
            {
                parent = t2Anchors[c2++].transform;
            }

            var viewer = Instantiate(playerViewer, parent);
            viewer.GetComponentsInChildren<Text>().First(e => e.name == "Pseudo").text = data.Pseudo;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void AddPlayerServerRpc(ulong playerId, ConnectionData data)
    {
        PlayersRegistry[playerId] = data;
        if (IsClient)
            FillPlayerViewers();
        Debug.Log($"added player {playerId}");
    }

    [ServerRpc(RequireOwnership = false)]
    private void RemovePlayerServerRpc(ulong playerId)
    {
        PlayersRegistry.Remove(playerId);
        if (IsClient)
            FillPlayerViewers();
    }

    public void StartGame()
    {
        NetworkManager.Singleton.SceneManager.LoadScene("GameScene", LoadSceneMode.Single);
    }
}