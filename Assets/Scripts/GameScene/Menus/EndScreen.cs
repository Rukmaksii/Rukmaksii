using System.Collections.Generic;
using System.Diagnostics;
using GameManagers;
using Map;
using model;
using model.Network;
using PlayerControllers;
using UnityEngine;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;

namespace GameScene.Menus
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

            DisplayStats();
        }
        
        // Update is called once per frame
        void Update()
        {
        }
        
        public void OnQuit()
        {
            Application.Quit();
        }

        private void DisplayWinner()
        {
        }

        private void DisplayStats()
        {
            BasePlayer[] players = FindObjectsOfType<BasePlayer>();
            
            foreach (ulong player in _scoreboard.Keys)
            {
                BaseEntry baseEntry = Instantiate(baseEntryPrefab, holderScoreboard.transform).GetComponent<BaseEntry>();
                baseEntry.Player = player;
                baseEntry.Init();
            }
        }
    }
}