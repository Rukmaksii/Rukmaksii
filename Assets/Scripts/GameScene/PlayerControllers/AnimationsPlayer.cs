using Unity.Netcode;
using UnityEngine;

namespace GameScene.PlayerControllers
{
    public class AnimationsPlayer : NetworkBehaviour
    {
        private Animator playerAnimator;

        private BasePlayer.BasePlayer player;

        private bool fall;
        private bool _Enabled;
        private float TimeFromBoolStart = 0;

        // Start is called before the first frame update
        void Start()
        {
            playerAnimator = GetComponent<Animator>();
            player = GetComponentInParent<BasePlayer.BasePlayer>();
        }

        // Update is called once per frame
        void Update()
        {
            fall = !player.IsGrounded;

            if (fall && !_Enabled)
            {
                _Enabled = true;
                TimeFromBoolStart = Time.realtimeSinceStartup;
            }

            _Enabled = fall;

            Vector3 velocity = player.Velocity;
            playerAnimator.SetBool("fly", player.IsFlying);

            playerAnimator.SetBool("isSprinting", player.IsRunning);

            playerAnimator.SetBool("jump", velocity.y > 0);

            if (playerAnimator.GetBool("grounded") && !playerAnimator.GetBool("jump"))
            {
                playerAnimator.SetBool("grounded", GetTimeSinceBool() < 0.5);
            }
            else
            {
                playerAnimator.SetBool("grounded", player.IsGrounded);
            }


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

        //Calculates how much time has the player been in the air
        private float GetTimeSinceBool()
        {
            if (fall)
            {
                return Time.realtimeSinceStartup - TimeFromBoolStart;
            }
            else
            {
                return 0;
            }
        }
    }
}