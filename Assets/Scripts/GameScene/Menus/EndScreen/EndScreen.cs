using System;
using System.Collections.Generic;
using System.Linq;
using GameScene.GameManagers;
using GameScene.Map;
using GameScene.model.Network;
using LobbyScene;
using UnityEngine;
using UnityEngine.UI;

namespace GameScene.Menus.EndScreen
{
    public class EndScreen : MonoBehaviour
    {
        [SerializeField] private Text winner;
        [SerializeField] private GameObject holderScoreboard;
        [SerializeField] private GameObject baseEntryPrefab;
        
        private NetworkScoreboard _scoreboard;

        // Start is called before the first frame update
        void Start()
        {
            Cursor.lockState = CursorLockMode.Confined;

            _scoreboard = GameController.Singleton.Scoreboard;

            DisplayWinner();
            DisplayStats();
        }
        
        // Update is called once per frame
        void Update()
        {
        }
        
        public void OnQuit()
        {
            FindObjectOfType<LobbyManager>().UnloadGame();
        }

        private void DisplayWinner()
        {
            if (GameController.Singleton.winningTeam == 0)
                winner.text = "Blue team wins";
            else
                winner.text = "Red team wins";
        }

        private void DisplayStats()
        {
            
            List<ulong> orderPlayers =  _scoreboard.Keys.OrderByDescending(key =>
            {
                var fields = _scoreboard[key];
                return !fields.ContainsKey(PlayerInfoField.Kill) ? 0 : fields[PlayerInfoField.Kill];
            }).ToList();

            foreach (ulong p in orderPlayers)
            {
                BaseEntry baseEntry = Instantiate(baseEntryPrefab, holderScoreboard.transform).GetComponent<BaseEntry>();
                baseEntry.PlayerId = p;
            }
        }
    }
}
