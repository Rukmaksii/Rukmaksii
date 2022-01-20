using System.Collections;
using System.Collections.Generic;
using PlayerControllers;
using UnityEngine;

public class AnimationsPlayer : MonoBehaviour
{
    private Animator playerAnimator;
    private BasePlayer player;
    // Start is called before the first frame update
    void Start()
    {
        playerAnimator = GetComponent<Animator>();
        player = GetComponentInParent<BasePlayer>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.Z))
        {
            playerAnimator.SetBool("isMoving", true);
        }
        else if (Input.GetKey(KeyCode.S))
        {
            playerAnimator.SetBool("isMoving", true);
        }
        else if (Input.GetKey(KeyCode.Q))
        {
            playerAnimator.SetBool("isMoving", true);
        }
        else if (Input.GetKey(KeyCode.D))
        {
            playerAnimator.SetBool("isMoving", true);
        }
        else
        {
            playerAnimator.SetBool("isMoving", false);
        }

        if (player.IsRunning)
        {
            playerAnimator.SetBool("isSprinting", true);
        }
        else
        {
            playerAnimator.SetBool("isSprinting", false);
        }
    }
}
