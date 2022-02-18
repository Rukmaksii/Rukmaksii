using System.Collections;
using System.Collections.Generic;
using PlayerControllers;
using Unity.Netcode;
using UnityEngine;

public class AnimationsPlayer : NetworkBehaviour
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
        if (!IsServer)
            return;
        Vector3 velocity = player.Velocity;
        playerAnimator.SetBool("fly", player.IsFlying);
        
        playerAnimator.SetBool("isSprinting", player.IsRunning);

        playerAnimator.SetBool("jump", velocity.y >= 0.3);

        playerAnimator.SetBool("grounded", player.IsGrounded);

        playerAnimator.SetBool("forward", false);
        playerAnimator.SetBool("backward", false);
        playerAnimator.SetBool("left", false);
        playerAnimator.SetBool("right", false);
        
        if (velocity.z >= 0.7) //forward
        {
            playerAnimator.SetBool("forward", true);
        }
        else if (velocity.z <= -0.7) //backward
        {
            playerAnimator.SetBool("backward", true);
        }
        else if (velocity.x < -0.7) //left
        {
            playerAnimator.SetBool("left", true);
        }
        else if (velocity.x > 0.7) //right
        {
            playerAnimator.SetBool("right", true);
        }
    }
}