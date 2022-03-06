using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Items;
using JetBrains.Annotations;
using model;
using UnityEngine;
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

        private void Update()
        {
            GameObject[] playersArray = GameObject.FindGameObjectsWithTag("Player");
            foreach (var player in playersArray)
            {
                BasePlayer basePlayer = player.GetComponent<BasePlayer>();
                if (basePlayer.CurrentHealthValue <= 0)
                {
                    player.SetActive(false);
                    /*
                    CharacterController cc = basePlayer.GetComponent(typeof(CharacterController)) as CharacterController;
                    cc.enabled = false;
                    
                    // respawn position
                    player.GetComponent<BasePlayer>().UpdatePositionServerRpc(new Vector3(0f, 1f, 0f));
                    
                    StartCoroutine(RespawnTimer(player, basePlayer));
                    */
                    
                    Debug.Log("killed");
                    StartCoroutine(RespawnTimer(player));
                }
                else
                {
                    player.SetActive(true);
                }
            }
        }

        public void OnKilled(BasePlayer player)
        {
            //
        }

        IEnumerator RespawnTimer(GameObject player)
        {
            BasePlayer basePlayer = player.GetComponent<BasePlayer>();
            yield return new WaitForSeconds(5);
            player.SetActive(true);
            basePlayer.UpdateHealthServerRpc(basePlayer.MaxHealth, basePlayer.OwnerClientId);
            
            CharacterController cc = basePlayer.GetComponent(typeof(CharacterController)) as CharacterController;
            cc.enabled = false;
            
            basePlayer.OnRespawn();
            
            cc.enabled = true;
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