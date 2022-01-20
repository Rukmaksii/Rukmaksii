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
        Vector3 velocity = Vector3.ClampMagnitude(player.RigidBody.transform.InverseTransformDirection(player.RigidBody.velocity), 1f);
        if (velocity.z >= 0.7) //forward
        {
            playerAnimator.SetBool("isMoving", true);
        }
        else if (velocity.z <= -0.7) //backward
        {
            playerAnimator.SetBool("isMoving", true);
        }
        else if (velocity.x < -0.7) //left
        {
            playerAnimator.SetBool("isMoving", true);
        }
        else if (velocity.x > 0.7) //right
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
