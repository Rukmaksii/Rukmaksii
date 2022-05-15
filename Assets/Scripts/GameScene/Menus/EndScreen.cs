using System;
using System.Collections.Generic;
using System.Diagnostics;
using GameManagers;
using Map;
using model;
using model.Network;
using PlayerControllers;
using UnityEngine;
using UnityEngine.UI;

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
            List<Tuple<ulong,int>> players = new List<Tuple<ulong,int>>();
            
            foreach (ulong player in _scoreboard.Keys)
            {
                if (players.Count == 0 || !_scoreboard[player].ContainsKey(PlayerInfoField.Kill))
                    players.Add(new Tuple<ulong, int>(player,
                        _scoreboard[player].ContainsKey(PlayerInfoField.Kill) ? _scoreboard[player][PlayerInfoField.Kill] : 0));
                else
                {
                    int i = 0;
                    while (i < players.Count)
                    {
                        if (_scoreboard[player][PlayerInfoField.Kill] >= players[i].Item2)
                        {
                            players.Insert(i,new Tuple<ulong, int>(player, _scoreboard[player][PlayerInfoField.Kill]));
                            break;
                        }

                        i++;
                    }
                }
            }
            
            foreach (Tuple<ulong,int> p in players)
            {
                BaseEntry baseEntry = Instantiate(baseEntryPrefab, holderScoreboard.transform).GetComponent<BaseEntry>();
                baseEntry.Player = p.Item1;
                baseEntry.Init();
            }
        }
    }
}
