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
        Vector3 velocity = player.Velocity;

        if (velocity.y >= 0.3) //jump
        {
            playerAnimator.SetBool("jump", true);
        }
        else
        {
            playerAnimator.SetBool("jump", false);
        }
        if (velocity.z >= 0.7) //forward
        {
            playerAnimator.SetBool("forward", true);
            playerAnimator.SetBool("backward", false);
            playerAnimator.SetBool("left", false);
            playerAnimator.SetBool("right", false);
        }
        else if (velocity.z <= -0.7) //backward
        {
            playerAnimator.SetBool("backward", true);
            playerAnimator.SetBool("forward", false);
            playerAnimator.SetBool("left", false);
            playerAnimator.SetBool("right", false);
        }
        else if (velocity.x < -0.7) //left
        {
            playerAnimator.SetBool("left", true);
            playerAnimator.SetBool("forward", false);
            playerAnimator.SetBool("backward", false);
            playerAnimator.SetBool("right", false);
        }
        else if (velocity.x > 0.7) //right
        {
            playerAnimator.SetBool("right", true);
            playerAnimator.SetBool("forward", false);
            playerAnimator.SetBool("backward", false);
            playerAnimator.SetBool("left", false);
        }
        else
        {
            playerAnimator.SetBool("forward", false);
            playerAnimator.SetBool("backward", false);
            playerAnimator.SetBool("left", false);
            playerAnimator.SetBool("right", false);
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
