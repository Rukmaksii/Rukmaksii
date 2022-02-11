using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using model;
using UnityEngine;
using PlayerControllers;
using UnityEngine.AI;


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
        
        
        [SerializeField] protected MonsterControler monster;
        private MonsterControler monsterinstance;

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

        IEnumerator waitagent()
        {
            yield return new WaitForSeconds(10);
            monsterinstance = Instantiate(monster);
            Vector3 sourcePostion = new Vector3( 15, -0.01f, -6 );//The position you want to place your agent
            NavMeshHit closestHit;
            NavMesh.SamplePosition(sourcePostion, out closestHit, 500, 2);
            monsterinstance.transform.position = sourcePostion;
            yield return new WaitForSeconds(5);
            monsterinstance.gameObject.AddComponent<NavMeshAgent>();
        }
    }
}