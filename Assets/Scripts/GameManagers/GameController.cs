using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Items;
using JetBrains.Annotations;
using model;
using UnityEngine;
using UnityEngine.UI;
using PlayerControllers;
using UnityEngine.AI;
using Unity.Netcode;
using UnityEngine.PlayerLoop;
using UnityEngine.Purchasing;


namespace GameManagers
{
    
    enum GameState
    {
        Menu,
        Playing,
        Ended
    }

    public class GameController : MonoBehaviour
    {
        private List<BasePlayer> players = new List<BasePlayer>();

        [SerializeField] private List<GameObject> weaponPrefabs = new List<GameObject>();

        public List<GameObject> WeaponPrefabs => weaponPrefabs;
        
        [SerializeField] private List<GameObject> itemPrefabs = new List<GameObject>();

        public List<GameObject> ItemPrefabs => itemPrefabs;

        [SerializeField] private ConnectionScriptableObject connectionData;
        public ConnectionData Parameters => connectionData.Data;
        
        [SerializeField] protected GameObject uiPrefab;
        private GameObject playerUIInstance;
        
        [SerializeField] protected GameObject deathScreenPrefab;

        public GameObject deathScreen;

        private BasePlayer localPlayer;
        
        
        [SerializeField] protected GameObject monster;
        private GameObject monsterinstance;

        [SerializeField] private int respawnTime = 5;
        
        public BasePlayer LocalPlayer => localPlayer;
        
        public void BindPlayer(BasePlayer player)
        {
            localPlayer = player;
            players.Append(player);
        }

        void Start()
        {
            playerUIInstance = Instantiate(uiPrefab);
            playerUIInstance.name = uiPrefab.name;
            playerUIInstance.GetComponent<Canvas>().worldCamera = Camera.current;
            
            deathScreen = Instantiate(deathScreenPrefab);
            deathScreen.name = deathScreenPrefab.name;
            deathScreen.GetComponent<Canvas>().worldCamera = Camera.current;
            deathScreen.SetActive(false);
            
            StartCoroutine(Waitagent());
        }

        private void Update()
        {
            GameObject[] playersArray = GameObject.FindGameObjectsWithTag("Player");
            foreach (var player in playersArray)
            {
                BasePlayer basePlayer = player.GetComponent<BasePlayer>();
                if (basePlayer.CurrentHealthValue <= 0)
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

        IEnumerator DeathScreenTimer()
        {
            for (int i = respawnTime; i > 0; i--)
            {
                deathScreen.GetComponent<Canvas>().GetComponentsInChildren<Text>()[1].text = $"Respawning in {i} seconds";
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
                basePlayer.UpdateHealthServerRpc(basePlayer.MaxHealth, basePlayer.OwnerClientId);
            
                CharacterController cc = basePlayer.GetComponent(typeof(CharacterController)) as CharacterController;
                cc.enabled = false;
            
                basePlayer.OnRespawn();
            
                cc.enabled = true;
            }
        }
        
        IEnumerator Waitagent()
        {
            yield return new WaitForSeconds(0);
            for (int i = 0; i < 4; i++)
            {
                monsterinstance = Instantiate(monster);
                Vector3 sourcePostion = new Vector3(15 * i, -25, -20); //The position you want to place your agent
                NavMeshHit closestHit;
                NavMesh.SamplePosition(sourcePostion, out closestHit, 500, 2);
                monsterinstance.transform.position = sourcePostion;
                yield return new WaitForSeconds(15);
                monsterinstance.gameObject.AddComponent<NavMeshAgent>();
            }
        }
    }
}