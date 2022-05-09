using System.Collections;
using System.Collections.Generic;
using HUD;
using Items;
using Map;
using Minions;
using model;
using model.Network;
using PlayerControllers;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using Weapons;

namespace GameManagers
{
    enum GameState
    {
        Menu,
        Playing,
        Ended
    }


    [RequireComponent(typeof(NetworkObject))]
    [RequireComponent(typeof(Gameloop))]
    public class GameController : NetworkBehaviour
    {
        public static GameController Singleton { get; private set; }

        private List<BasePlayer> players = new List<BasePlayer>();

        public readonly NetworkScoreboard Scoreboard = new NetworkScoreboard();

        public List<BasePlayer> Players
        {
            get
            {
                players = players.FindAll(p => p != null && p.gameObject != null);
                return players;
            }
        }

        private List<BaseMinion> minions = new List<BaseMinion>();

        public List<BaseMinion> Minions
        {
            get
            {
                minions = minions.FindAll(m => m != null && m.gameObject != null);
                return minions;
            }
        }


        public Vector3 SpawnPoint { get; private set; }


        [SerializeField] private List<GameObject> classPrefabs = new List<GameObject>();

        public List<GameObject> ClassPrefabs => classPrefabs;
        [SerializeField] private List<GameObject> weaponPrefabs = new List<GameObject>();

        public List<GameObject> WeaponPrefabs => weaponPrefabs;

        [SerializeField] private List<GameObject> itemPrefabs = new List<GameObject>();

        public List<GameObject> ItemPrefabs => itemPrefabs;

        [SerializeField] private ConnectionScriptableObject connectionData;
        public ConnectionData Parameters => connectionData.Data;

        public GameObject PlayerUIInstance { get; private set; }

        [SerializeField] protected GameObject deathScreenPrefab;

        #region Prefabs

        [SerializeField] private GameObject minionPrefab;

        public GameObject MinionPrefab => minionPrefab;

        [SerializeField] private GameObject monsterPrefab;

        public GameObject MonsterPrefab => monsterPrefab;

        #endregion

        public GameObject deathScreen;

        private BasePlayer localPlayer;


        [SerializeField] private int respawnTime = 5;

        public BasePlayer LocalPlayer => localPlayer;

        public HUDController HUDController => HUDController.Singleton;


        private Queue<BasePlayer> playersToEquip = new Queue<BasePlayer>();

        private void Awake()
        {
            if (Singleton != null && Singleton != this)
            {
                Destroy(this);
                return;
            }

            Singleton = this;
        }

        /**
         * <summary>binds the local player to the game controller</summary>
         */
        private void BindPlayer(BasePlayer player)
        {
            localPlayer = player;

            int teamId = Parameters.IsReady
                ? Parameters.TeamId
                : 0;
            player.UpdateTeamServerRpc(teamId);
        }

        /**
         * <summary>adds a player to the game controller</summary>
         */
        public void AddPlayer(BasePlayer player)
        {
            if (player.IsOwner)
                BindPlayer(player);
            players.Add(player);
        }

        /**
         * <summary>adds a <see cref="BaseMinion"/> to the registry</summary>
         */
        public void RegisterMinion(BaseMinion minion)
        {
            minions.Add(minion);
        }

        void Start()
        {
            GameObject.Find("Base1").GetComponent<BaseController>().UpdateTeamServerRpc(0);
            GameObject.Find("Base2").GetComponent<BaseController>().UpdateTeamServerRpc(1);

            GameObject.Find("Shield1").GetComponent<ShieldController>().UpdateTeamServerRpc(0);
            GameObject.Find("Shield2").GetComponent<ShieldController>().UpdateTeamServerRpc(1);


            PlayerUIInstance = GameObject.FindWithTag("PlayerHUD");
            //PlayerUIInstance.SetActive(false);

            deathScreen = Instantiate(deathScreenPrefab);
            deathScreen.name = deathScreenPrefab.name;
            deathScreen.GetComponent<Canvas>().worldCamera = Camera.current;
            deathScreen.SetActive(false);
        }

        private void Update()
        {
            ManageDeath();
            if (IsServer)
                UpdateServer();
        }

        void UpdateServer()
        {
            BasePlayer player;
            while (playersToEquip.Count > 0 && (player = playersToEquip.Peek()).IsSpawned)
            {
                EquipPlayer(player);
                playersToEquip.Dequeue();
            }
        }

        public override void OnNetworkSpawn()
        {
            SetSpawnPoint();
            string className = connectionData.Data?.ClassName ?? classPrefabs[0].GetComponent<BasePlayer>().ClassName;
            if (IsClient)
                SpawnPlayerServerRpc(className, NetworkManager.Singleton.LocalClientId,
                    SpawnPoint,
                    Quaternion.identity);
        }

        private void SetSpawnPoint()
        {
            if (Parameters.TeamId == 1)
            {
                GameObject obj = GameObject.FindGameObjectWithTag("SpawnPoint2");
                SpawnPoint = obj.transform.position;
            }
            else
            {
                GameObject obj = GameObject.FindGameObjectWithTag("SpawnPoint1");
                SpawnPoint = obj.transform.position;
            }
        }

        [ServerRpc(RequireOwnership = false)]
        private void SpawnPlayerServerRpc(string className, ulong ownerId, Vector3 position, Quaternion rotation)
        {
            GameObject playerPrefab = classPrefabs.Find(go =>
                go.GetComponent<BasePlayer>().ClassName == className);

            GameObject instance = Instantiate(playerPrefab, position, rotation);

            var netObj = instance.GetComponent<NetworkObject>();
            netObj.SpawnAsPlayerObject(ownerId, true);

            var player = instance.GetComponent<BasePlayer>();

            playersToEquip.Enqueue(player);
        }

        private void ManageDeath()
        {
            GameObject[] playersArray = GameObject.FindGameObjectsWithTag("Player");
            foreach (var player in playersArray)
            {
                BasePlayer basePlayer = player.GetComponent<BasePlayer>();
                if (basePlayer.CurrentHealth <= 0)
                {
                    if (basePlayer == LocalPlayer)
                    {
                        deathScreen.SetActive(true);
                        StartCoroutine(DeathScreenTimer());
                    }


                    player.SetActive(false);

                    StartCoroutine(RespawnTimer(player));
                }
                else
                {
                    player.SetActive(true);
                }
            }
        }

        private void EquipPlayer(BasePlayer player)
        {
            if (!IsServer)
                throw new NotServerException();

            player.CurrentHealth = player.MaxHealth;
            player.Money = 500;
            GameObject autoWeaponPrefab =
                WeaponPrefabs.Find(go => go.name == "TestAutoPrefab");
            GameObject weaponInstance = Instantiate(autoWeaponPrefab);
            var netObj = weaponInstance.GetComponent<NetworkObject>();
            netObj.Spawn(true);
            player.Inventory.AddWeapon(weaponInstance.GetComponent<BaseWeapon>());


            GameObject gunWeaponPrefab =
                WeaponPrefabs.Find(go => go.name == "TestGunPrefab");
            weaponInstance = Instantiate(gunWeaponPrefab);
            weaponInstance.GetComponent<NetworkObject>().Spawn(true);
            player.Inventory.AddWeapon(weaponInstance.GetComponent<BaseWeapon>());

            GameObject fuelBoosterPrefab =
                ItemPrefabs.Find(go => go.name == "FuelBoosterPrefab");
            GameObject grenadePrefab =
                ItemPrefabs.Find(go => go.name == "Grenade");
            FuelBooster itemInstance = Instantiate(fuelBoosterPrefab).GetComponent<FuelBooster>();
            for (int i = 0; i < 10; i++)
            {
                Grenade grenade = Instantiate(grenadePrefab).GetComponent<Grenade>();
                grenade.NetworkObject.Spawn(true);
                player.Inventory.AddItem(grenade);
            }

            itemInstance.NetworkObject.Spawn(true);
            player.Inventory.AddItem(itemInstance);
        }

        IEnumerator DeathScreenTimer()
        {
            for (int i = respawnTime; i > 0; i--)
            {
                deathScreen.GetComponent<Canvas>().GetComponentsInChildren<Text>()[1].text =
                    $"Respawning in {i} seconds";
                yield return new WaitForSeconds(1);
            }
        }

        IEnumerator RespawnTimer(GameObject player)
        {
            BasePlayer basePlayer = player.GetComponent<BasePlayer>();
            yield return new WaitForSeconds(respawnTime);

            if (!player.activeSelf)
            {
                player.SetActive(true);
                basePlayer.CurrentHealth = basePlayer.MaxHealth;

                basePlayer.OnRespawn();
            }
        }
    }
}