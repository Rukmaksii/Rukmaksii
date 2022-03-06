using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using model;
using UnityEngine;
using PlayerControllers;
using UnityEngine.AI;
using Unity.Netcode;
using UnityEngine.PlayerLoop;


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

        private BasePlayer localPlayer;
        
        
        [SerializeField]protected GameObject monster;
        private GameObject monsterinstance;

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
            
            StartCoroutine(waitagent());
        }

        public void OnKilled(BasePlayer deadPlayer)
        {
            GameObject[] playersArray = GameObject.FindGameObjectsWithTag("Player");
            foreach (var player in playersArray)
            {
                BasePlayer basePlayer = player.GetComponent<BasePlayer>();
                if (basePlayer.OwnerClientId == deadPlayer.OwnerClientId)
                {
                    // death room position
                    basePlayer.UpdatePositionServerRpc(new Vector3(0f, -45f, 0f));
                    
                    player.GetComponent<BasePlayer>().UpdateHealthServerRpc(basePlayer.MaxHealth, basePlayer.OwnerClientId);
                    
                    StartCoroutine(respawnTimer(player));
                }
            }
        }

        IEnumerator respawnTimer(GameObject player)
        {
            player.SetActive(false);
            yield return new WaitForSeconds(0.1f);
            player.SetActive(true);
            yield return new WaitForSeconds(0.1f);
            player.SetActive(false);
            yield return new WaitForSeconds(3);
            player.SetActive(true);
            yield return new WaitForSeconds(0.1f);
            
            // respawn position
            player.GetComponent<BasePlayer>().UpdatePositionServerRpc(new Vector3(0f, 1f, 0f));
        }
        
        IEnumerator waitagent()
        {
            yield return new WaitForSeconds(0);
            for (int i = 0; i < 4; i++)
            {
                monsterinstance = Instantiate(monster);
                Vector3 sourcePostion = new Vector3(15 * i, -0.01f, -6); //The position you want to place your agent
                NavMeshHit closestHit;
                NavMesh.SamplePosition(sourcePostion, out closestHit, 500, 2);
                monsterinstance.transform.position = sourcePostion;
                yield return new WaitForSeconds(5);
                monsterinstance.gameObject.AddComponent<NavMeshAgent>();
            }
        }
    }
}