using System.Collections.Generic;
using GameManagers;
using model;
using Unity.Netcode;
using UnityEngine;

public class LobbyManager : NetworkBehaviour
{
    [SerializeField] private ConnectionManagerScript connectionManager;

    [SerializeField] private ConnectionScriptableObject connectionData;
    [SerializeField] private GameObject playerViewer;

    private NetworkVariable<int> playerCount = new NetworkVariable<int>();

    public int PlayerCount => playerCount.Value;

    // Start is called before the first frame update
    void Start()
    {
        if (IsServer)
        {
            playerCount.Value = connectionData.Data.PlayerAmount;
        }

        NetworkManager.Singleton.OnClientConnectedCallback += delegate(ulong clientId)
        {
            if (!NetworkManager.Singleton.IsServer)
                return;
            Debug.Log($"{clientId} connected");
        };
    }

    // Update is called once per frame
    void Update()
    {
    }
}