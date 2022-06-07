using GameScene.Minions;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

namespace GameScene.PlayerControllers
{
    public class AnimationsMinion : NetworkBehaviour
    {
        private Animator playerAnimator;

        private NavMeshAgent player;

        // Start is called before the first frame update
        void Start()
        {
            playerAnimator = GetComponent<Animator>();
            player = GetComponentInParent<NavMeshAgent>();
        }

        // Update is called once per frame
        void Update()
        {
            Vector3 velocity = player.velocity;

            bool forw = (velocity.x != 0) && (velocity.y != 0) && (velocity.z != 0);
            
            
            playerAnimator.SetBool("forward", forw);
        }
    }
}