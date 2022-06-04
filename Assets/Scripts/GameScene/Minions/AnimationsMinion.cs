using GameScene.Minions;
using Unity.Netcode;
using UnityEngine;

namespace GameScene.PlayerControllers
{
    public class AnimationsMinion : NetworkBehaviour
    {
        private Animator playerAnimator;

        private Rigidbody player;

        private Vector3 prevVelocity = Vector3.zero;

        // Start is called before the first frame update
        void Start()
        {
            playerAnimator = GetComponent<Animator>();
            player = GetComponentInParent<Rigidbody>();
            
            playerAnimator.SetBool("fly", false);
            playerAnimator.SetBool("isSprinting", false);
            playerAnimator.SetBool("grounded", true);
            playerAnimator.SetBool("jump", false);
            playerAnimator.SetBool("backward", false);
            playerAnimator.SetBool("left", false);
            playerAnimator.SetBool("right", false);
        }

        // Update is called once per frame
        void Update()
        {
            Vector3 velocity = player.transform.localPosition;
            
            bool forw = (velocity.x - prevVelocity.x != 0) && (velocity.y - prevVelocity.y != 0) && (velocity.z - prevVelocity.z != 0);
            
            
            playerAnimator.SetBool("forward", forw);

            prevVelocity = velocity;
        }
    }
}