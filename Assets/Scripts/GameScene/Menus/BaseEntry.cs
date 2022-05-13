using System.Collections.Generic;
using System.Diagnostics;
using GameManagers;
using Map;
using model;
using model.Network;
using PlayerControllers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;

public class BaseEntry : MonoBehaviour
{
    [SerializeField] private Text killField;
    [SerializeField] private Text nameField;
    [SerializeField] private Text damageField;
    
    [SerializeField] private Image backField;
    [SerializeField] private Image backName;
    [SerializeField] private Image backKill;
    [SerializeField] private Image backDamage;

    public ulong Player;
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
            if (p.OwnerClientId == Player)
                team = p.TeamId;

        if (team == 0)
        {
            backField.color = new Color(0.1568f,0.7843137f,1f, 1);
            backName.color = new Color(0.1960f,0.4509f,0.9411f, 1);
            backKill.color = new Color(0.3333f,0.5294f,0.8627f, 1);
            backDamage.color = new Color(0.3333f,0.5294f,0.8627f, 1);
        }
        else
        {
            backField.color = new Color(0.8962f,0.3339f,0.3339f, 1);
            backName.color = new Color(0.9339f,0.1630f,0.1630f, 1);
            backKill.color = new Color(0.9725f,0.4823f,0.4823f, 1);
            backDamage.color = new Color(0.9725f,0.4823f,0.4823f, 1);
        }
        
        nameField.text = "Bob";
       
        if (_scoreboard[Player].ContainsKey(PlayerInfoField.Kill))
            killField.text = $"{_scoreboard[Player][PlayerInfoField.Kill]}";
        else
            killField.text = "0";

        if (_scoreboard[Player].ContainsKey(PlayerInfoField.DamagesDone))
            damageField.text = $"{_scoreboard[Player][PlayerInfoField.DamagesDone]}";
        else
            damageField.text = "0";
    }
}
