using System.Collections.Generic;
using System.Linq;
using model;
using UnityEngine;
using PlayerControllers;


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
        }
    }
}