using System.Collections;
using System.Collections.Generic;
using UnityEngine;



enum GameState
{
    Menu,
    Playing,
    Ended
}

public class GameController : MonoBehaviour
{
    
    private List<PlayerController> players = new List<PlayerController>();

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
