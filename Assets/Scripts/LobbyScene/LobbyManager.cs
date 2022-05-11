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
    [SerializeField] private List<GameObject> classPrefabs = new List<GameObject>();
    public List<GameObject> ClassPrefabs => classPrefabs;

    [SerializeField] private ConnectionManagerScript connectionManager;

    [SerializeField] private ConnectionScriptableObject connectionData;
    [SerializeField] private GameObject playerViewer;

    // a prefab for class selection in lobby
    [SerializeField] private GameObject classCanvas;
    [SerializeField] private RectTransform classViewport;

    [SerializeField] private Button startButton;
    [SerializeField] private Canvas lobbyUI;

    private NetworkVariable<int> playerCount = new NetworkVariable<int>();

    public readonly NetworkPlayersRegistry<ConnectionData> PlayersRegistry =
        new NetworkPlayersRegistry<ConnectionData>();

    public ConnectionData PlayerData => PlayersRegistry.ContainsKey(NetworkManager.Singleton.LocalClientId) ? PlayersRegistry[NetworkManager.Singleton.LocalClientId] : connectionData.Data;

    public int PlayerCount => playerCount.Value;

    public bool CanStart
    {
        get
        {
            if (PlayersRegistry.Count < PlayerCount)
                return false;
            int validPlayers = 0;
            var classNames = classPrefabs.Select(go => go.GetComponent<BasePlayer>().ClassName).ToList();
            foreach (var data in PlayersRegistry.Values)
            {
                if (classNames.Contains(data.ClassName))
                    validPlayers++;
            }

            return validPlayers == PlayerCount;
        }
    }


    public static LobbyManager Singleton { get; private set; }

    private void Awake()
    {
        if (Singleton != null && Singleton != this)
        {
            Destroy(this.gameObject);
            return;
        }

        Singleton = this;
    }

    private BasePlayer PlayerClass
    {
        get
        {
            if (PlayerData.ClassName == null)
                return null;
            return ClassPrefabs.Select(go => go.GetComponent<BasePlayer>())
                .FirstOrDefault(p => p.ClassName == PlayerData.ClassName);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        DontDestroyOnLoad(gameObject);
#if DEBUG
        // in game scene at startup
        if (lobbyUI == null)
            return;
#endif
        PlayersRegistry.OnValueChanged += (@event) => { FillPlayerViewers(); };
        NetworkManager.Singleton.OnClientConnectedCallback += (clientId) =>
        {
            if (!NetworkManager.Singleton.IsClient)
                return;

            if (NetworkManager.Singleton.IsServer)
            {
                connectionData.Data.TeamId = 0;
                PlayersRegistry[clientId] = connectionData.Data;
                FillPlayerViewers();
            }
            else
            {
                AddPlayerServerRpc(clientId, connectionData.Data);
            }
        };

        NetworkManager.Singleton.OnServerStarted += () =>
        {
            if (NetworkManager.Singleton.IsServer)
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
        };
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsSpawned || lobbyUI == null)
            return;
        if (IsServer)
        {
            startButton.gameObject.SetActive(true);
            startButton.interactable = startButton.enabled = CanStart;
        }
    }

    private List<ConnectionData> GetPlayersInTeam(int t) => PlayersRegistry.Values.Where(c => c.TeamId == t).ToList();

    private void FillPlayerViewers()
    {
        var anchors = lobbyUI.GetComponentsInChildren<RectTransform>().Where(t => t.name.StartsWith("Anchor")).ToList();
        anchors.ForEach(a =>
        {
            if (a.childCount == 1)
                Destroy(a.GetChild(0).gameObject);
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

            BasePlayer player;
            if ((player = PlayerClass) != null)
            {
                viewer.GetComponentsInChildren<Image>().First(e => e.name == "Image").sprite = player.Sprite;
            }
        }

        FillClassViewport();
    }

    private void FillClassViewport()
    {
        var usedClasses = GetPlayersInTeam(PlayerData.TeamId).Select(d => d.ClassName).ToList();
        var availableClasses = classPrefabs
            .Select(go => go.GetComponent<BasePlayer>())
            .Where(p => !usedClasses.Contains(p.ClassName))
            .ToList();

        for (int i = 0; i < classViewport.childCount; i++)
        {
            Destroy(classViewport.GetChild(i).gameObject);
        }

        float offset = 0;
        foreach (var player in availableClasses)
        {
            var cv = Instantiate(classCanvas, classViewport);
            cv.transform.localPosition += offset * Vector3.up;
            cv.GetComponent<Image>().sprite = player.Sprite;
            offset -= classCanvas.GetComponent<RectTransform>().rect.height - 50;
            cv.GetComponent<Button>().onClick.AddListener(delegate
            {
                if (NetworkManager.Singleton.IsServer)
                {
                    ChangeClass(NetworkManager.Singleton.LocalClientId, player.ClassName);
                    FillPlayerViewers();
                }
                else
                {
                    ChangeClassServerRpc(NetworkManager.Singleton.LocalClientId, player.ClassName);
                }
            });
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void AddPlayerServerRpc(ulong playerId, ConnectionData data)
    {
        if (GetPlayersInTeam(0).Count > GetPlayersInTeam(1).Count)
        {
            data.TeamId = 1;
        }
        else
        {
            data.TeamId = 0;
        }

        PlayersRegistry[playerId] = data;
        if (IsClient)
            FillPlayerViewers();
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

    /// <summary>
    ///     changes the class of <see cref="player"/>
    /// </summary>
    /// <param name="player">the player id</param>
    /// <param name="newClass">the name of the new class</param>
    /// <remarks>has to be executed on server</remarks>
    private void ChangeClass(ulong player, string newClass)
    {
        var usedClasses = GetPlayersInTeam(PlayersRegistry[player].TeamId).Select(d => d.ClassName).ToList();
        if (usedClasses.Contains(newClass))
            return;
        var data = PlayersRegistry[player];
        data.ClassName = newClass;
        PlayersRegistry[player] = data;
    }

    [ServerRpc(RequireOwnership = false)]
    private void ChangeClassServerRpc(ulong player, string newClass) => ChangeClass(player, newClass);
}