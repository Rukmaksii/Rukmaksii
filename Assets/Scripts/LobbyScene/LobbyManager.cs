using GameManagers;
using model;
using Unity.Netcode;
using UnityEngine;

public class LobbyManager : NetworkBehaviour
{
    [SerializeField] private ConnectionManagerScript connectionManager;

    [SerializeField] private ConnectionScriptableObject connectionData;
    [SerializeField] private GameObject playerViewer;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
    }
}