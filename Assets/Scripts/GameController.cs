using System.Collections.Generic;
using System.Linq;
using UnityEngine;



enum GameState
{
    Menu,
    Playing,
    Ended
}

[RequireComponent(typeof(PlayerController))]

public class GameController : MonoBehaviour
{
    
    private List<PlayerController> players = new List<PlayerController>();

    [SerializeField] protected GameObject uiPrefab;
    private GameObject playerUIInstance;

    private PlayerController localPlayer;

    public PlayerController LocalPlayer => localPlayer;
    
    public void BindPlayer(PlayerController player)
    {
        localPlayer = player;
        players.Append(player);
    }
    
    void Start()
    {
        playerUIInstance = Instantiate(uiPrefab);
        playerUIInstance.name = uiPrefab.name;
        
        playerUIInstance.GetComponent<Canvas>().worldCamera = Camera.current;

        HUDController ui = playerUIInstance.GetComponent<HUDController>();
        if (ui == null)
            Debug.LogError(("no ui"));
        ui.SetController(GetComponent<PlayerController>());
    }
}
