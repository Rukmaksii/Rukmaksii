using System;
using System.Collections.Generic;
using System.Linq;
using GameScene.GameManagers;
using GameScene.PlayerControllers.BasePlayer;
using model;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace LobbyScene
{
    public class LobbyManager : NetworkBehaviour
    {
        [SerializeField] private List<GameObject> classPrefabs = new List<GameObject>();
        public List<GameObject> ClassPrefabs => classPrefabs;

        [SerializeField] private ConnectionManagerScript connectionManager;

        [SerializeField] private ConnectionScriptableObject connectionData;
        [SerializeField] private GameObject playerViewer;

        // a prefab for class selection in lobby
        [SerializeField] private GameObject classCanvas;
        [SerializeField] private GameObject classDepoCanvas;
        [SerializeField] private RectTransform classViewport;
        [SerializeField] private Text roomName;

        [SerializeField] private Button startButton;
        [SerializeField] private Canvas lobbyUI;

        private string _roomName;


#if UNET
        private bool _bypassVerification = false;
#endif

        private NetworkVariable<GameState> gameState = new NetworkVariable<GameState>(GameState.Lobby);

        public GameState GameState
        {
            get => gameState.Value;
            private set
            {
                if (!IsServer)
                    throw new NotServerException($"client cannot set game state");
                gameState.Value = value;
            }
        }

        private NetworkVariable<int> playerCount = new NetworkVariable<int>();

        public readonly NetworkPlayersRegistry<ConnectionData> PlayersRegistry =
            new NetworkPlayersRegistry<ConnectionData>();

        public ConnectionData PlayerData => PlayersRegistry.ContainsKey(NetworkManager.Singleton.LocalClientId)
            ? PlayersRegistry[NetworkManager.Singleton.LocalClientId]
            : connectionData.Data;

        public int PlayerCount => playerCount.Value;

        public bool CanStart
        {
            get
            {
                if (DebugManager.ByPassCount)
                    return true;
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
            if (!NetworkManager.Singleton.IsServer)
                startButton.gameObject.SetActive(NetworkManager.Singleton.IsServer);

            NetworkManager.Singleton.ConnectionApprovalCallback +=
                delegate(byte[] data, ulong clientId, NetworkManager.ConnectionApprovedDelegate callback)
                {
                    bool approve = GameState == GameState.Lobby;


                    if (PlayersRegistry.Count >= 4)
                    {
                        playerCount.Value = 6;
                    }

                    approve &= PlayersRegistry.Count < PlayerCount;

#if UNET
                    approve |= _bypassVerification;
#endif

                    callback(false, null, approve, Vector3.zero, Quaternion.identity);
                };
#if UNET
            // in game scene at startup
            if (lobbyUI == null)
            {
                _bypassVerification = true;
                if (IsServer)
                    GameState = GameState.Playing;
                return;
            }
#endif
            PlayersRegistry.OnValueChanged += (@event) => { FillPlayerViewers(); };
            NetworkManager.Singleton.OnClientConnectedCallback += (clientId) =>
            {
                _roomName = connectionData.Data.RoomName;
                roomName.text = _roomName;
                if (!NetworkManager.Singleton.IsClient)
                    return;
                if (NetworkManager.Singleton.IsServer)
                {
                    connectionData.Data.TeamId = 0;
                    PlayersRegistry[clientId] = connectionData.Data;
                    if (GameState == GameState.Lobby)
                        FillPlayerViewers();
                }
                else
                {
                    AddPlayerServerRpc(clientId, connectionData.Data);
                }
            };

            NetworkManager.Singleton.OnClientDisconnectCallback += (clientId) =>
            {
                if (!NetworkManager.Singleton.IsServer || clientId == NetworkManager.ServerClientId)
                {
                    UnloadGame();
                }

                if (NetworkManager.Singleton.IsServer && GameState == GameState.Lobby)
                {
                    if (PlayersRegistry.Count == 4)
                        playerCount.Value = 4;
                    PlayersRegistry.Remove(clientId);
                    if (NetworkManager.Singleton.IsClient)
                        FillPlayerViewers();
                }
            };

            NetworkManager.Singleton.OnServerStarted += () =>
            {
                if (NetworkManager.Singleton.IsServer)
                {
                    playerCount.Value = connectionData.Data.PlayerAmount;
                }
            };
        }

        public void UnloadGame()
        {
            Cursor.lockState = CursorLockMode.None;
            SceneManager.LoadScene("ConnectionScene");
            Destroy(Singleton.gameObject);
            Destroy(ConnectionManagerScript.Singleton.gameObject);
            if (GameController.Singleton != null)
                Destroy(GameController.Singleton.gameObject);
            NetworkManager.Singleton.Shutdown();
        }

        // Update is called once per frame
        void Update()
        {
            if (!IsSpawned || lobbyUI == null)
                return;
            if (IsServer)
            {
                startButton.gameObject.SetActive(true);
                // TODO : removes can start by pass
                startButton.interactable = startButton.enabled = CanStart;
            }
        }

        private List<ConnectionData> GetPlayersInTeam(int t) =>
            PlayersRegistry.Values.Where(c => c.TeamId == t).ToList();

        private void FillPlayerViewers()
        {
            var anchors = lobbyUI.GetComponentsInChildren<RectTransform>().Where(t => t.name.StartsWith("Anchor"))
                .ToList();
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

                if (data.TeamId == 0)
                    viewer.transform.Find("BackgroundCircle").GetComponent<Image>().color =
                        new Color(0, 160f / 255, 1);
                else
                    viewer.transform.Find("BackgroundCircle").GetComponent<Image>().color =
                        new Color(226f / 255, 33f / 255, 0);

                BasePlayer player;
                if (data.ClassName != null && (player = ClassPrefabs.Select(go => go.GetComponent<BasePlayer>())
                        .FirstOrDefault(p => p.ClassName == data.ClassName)) != null)
                {
                    viewer.transform.Find("Image").transform.Find("BackgroundImage").GetComponent<Image>().sprite =
                        player.Sprite;
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


            float offset = -classCanvas.GetComponent<RectTransform>().rect.height / 2;
            foreach (var player in availableClasses)
            {
                var cv = Instantiate(classCanvas, classViewport);
                cv.transform.localPosition += offset * Vector3.up;
                cv.transform.Find("SpriteHolder").GetComponent<Image>().sprite = player.Sprite;
                offset -= classCanvas.GetComponent<RectTransform>().rect.height + 20;
                cv.GetComponent<Button>().onClick.AddListener(delegate
                {
                    ChangeClassServerRpc(NetworkManager.Singleton.LocalClientId, player.ClassName);
                });
                cv.transform.Find("ClassName").GetComponent<Text>().text = player.ClassName;
            }

            if (PlayerClass != null)
            {
                var cv = Instantiate(classDepoCanvas, classViewport);
                cv.transform.localPosition += offset * Vector3.up;
                cv.GetComponent<Button>().onClick.AddListener(delegate
                {
                    ChangeClassServerRpc(NetworkManager.Singleton.LocalClientId, null);
                });
            }

            classViewport.sizeDelta = new Vector2(classViewport.rect.width,
                -offset - classCanvas.GetComponent<RectTransform>().rect.height / 2);
        }

        [ServerRpc(RequireOwnership = false)]
        private void AddPlayerServerRpc(ulong playerId, ConnectionData data)
        {
            if (GetPlayersInTeam(1).Count >= GetPlayersInTeam(0).Count)
                data.TeamId = 0;
            else
                data.TeamId = 1;

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
            if (!IsServer)
                throw new NotServerException($"non-server client cannot start game");
            NetworkManager.Singleton.SceneManager.LoadScene("GameScene", LoadSceneMode.Single);
            GameState = GameState.Playing;
        }

        public void CopyRoomCode()
        {
            GUIUtility.systemCopyBuffer = _roomName;
        }

        /// <summary>
        ///     changes the class of <see cref="player"/>
        /// </summary>
        /// <param name="player">the player id</param>
        /// <param name="newClass">the name of the new class</param>
        /// <remarks>has to be executed on server</remarks>
        [ServerRpc(RequireOwnership = false)]
        private void ChangeClassServerRpc(ulong player, string newClass)
        {
            var usedClasses = GetPlayersInTeam(PlayersRegistry[player].TeamId).Select(d => d.ClassName).ToList();
            if (usedClasses.Contains(newClass))
                return;
            var data = PlayersRegistry[player];
            data.ClassName = newClass;
            PlayersRegistry[player] = data;
            if (NetworkManager.Singleton.IsClient)
                FillPlayerViewers();
        }

        public void OnQuit()
        {
            UnloadGame();
        }
    }
}