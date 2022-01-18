using System.Collections.Generic;
using System.Linq;
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