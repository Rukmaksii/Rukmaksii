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

}
