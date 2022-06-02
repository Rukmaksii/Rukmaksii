using System;
using GameScene.GameManagers;
using GameScene.model.Network;
using GameScene.PlayerControllers.BasePlayer;
using LobbyScene;
using UnityEngine;
using UnityEngine.UI;

namespace GameScene.Menus.EndScreen
{ 
    public class BaseEntry : MonoBehaviour
    {
        [SerializeField] private Text nameField;
        [SerializeField] private Text killField;
        [SerializeField] private Text deathField;
        [SerializeField] private Text ratioField;
        [SerializeField] private Text damageField;
        
        [SerializeField] private Image backField;
        [SerializeField] private Image backName;
        [SerializeField] private Image backKill;
        [SerializeField] private Image backDeath;
        [SerializeField] private Image backRatio;
        [SerializeField] private Image backDamage;

        public ulong playerId;
        private NetworkScoreboard _scoreboard;
        
        public void Init()
        {
            _scoreboard = GameController.Singleton.Scoreboard;
            UpdateInfos();
        }

        private void UpdateInfos()
        {
            int team = 0;
            foreach (BasePlayer p in GameController.Singleton.Players)
                if (p.OwnerClientId == playerId)
                    team = p.TeamId;

            if (team == 0)
            {
                backField.color = new Color(0.1568f,0.7843137f,1f, 1);
                backName.color = new Color(0.1960f,0.4509f,0.9411f, 1);
                backKill.color = new Color(0.3333f,0.5294f,0.8627f, 1);
                backDeath.color = new Color(0.3333f,0.5294f,0.8627f, 1);
                backRatio.color = new Color(0.3333f,0.5294f,0.8627f, 1);
                backDamage.color = new Color(0.3333f,0.5294f,0.8627f, 1);
            }
            else
            {
                backField.color = new Color(0.8962f,0.3339f,0.3339f, 1);
                backName.color = new Color(0.9339f,0.1630f,0.1630f, 1);
                backKill.color = new Color(0.9725f,0.4823f,0.4823f, 1);
                backDeath.color = new Color(0.9725f,0.4823f,0.4823f, 1);
                backRatio.color = new Color(0.9725f,0.4823f,0.4823f, 1);
                backDamage.color = new Color(0.9725f,0.4823f,0.4823f, 1);
            }

            string pseudo = LobbyManager.Singleton.PlayersRegistry[playerId].Pseudo;
            nameField.text = String.IsNullOrEmpty(pseudo) ? $"Player {playerId}" : pseudo;
           
            if (_scoreboard[playerId].ContainsKey(PlayerInfoField.Kill))
                killField.text = $"{_scoreboard[playerId][PlayerInfoField.Kill]}";
            else
                killField.text = "0";
            
            if (_scoreboard[playerId].ContainsKey(PlayerInfoField.Deaths))
                deathField.text = $"{_scoreboard[playerId][PlayerInfoField.Deaths]}";
            else
                deathField.text = "0";

            if (!_scoreboard[playerId].ContainsKey(PlayerInfoField.Kill))
                ratioField.text = "0";
            else if (!_scoreboard[playerId].ContainsKey(PlayerInfoField.Deaths))
                ratioField.text = $"{_scoreboard[playerId][PlayerInfoField.Kill]}";
            else
                ratioField.text = $"{_scoreboard[playerId][PlayerInfoField.Kill] / _scoreboard[playerId][PlayerInfoField.Deaths]}";

            if (_scoreboard[playerId].ContainsKey(PlayerInfoField.DamagesDone))
                damageField.text = $"{_scoreboard[playerId][PlayerInfoField.DamagesDone]}";
            else
                damageField.text = "0";
        }
    }
}