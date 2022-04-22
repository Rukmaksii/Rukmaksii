using GameManagers;
using model;
using Unity.Netcode;
using Unity.Netcode.Samples;
using UnityEngine;
using UnityEngine.Animations.Rigging;

namespace PlayerControllers
{
    enum PlayerFlags
    {
        MOVING = 1,
        RUNNING = 2 * MOVING,
        FLYING = 2 * RUNNING,
        SHOOTING = 2 * FLYING,
        DASHING = 2 * SHOOTING,
        AIMING = 2 * DASHING
    }

    /**
 * <summary>
 *      The controller class for any player spawned in the game
 * </summary>
 */
    [RequireComponent(typeof(Collider))]
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(NetworkObject))]
    [RequireComponent(typeof(ClientNetworkTransform))]
    [RequireComponent(typeof(CooldownManager))]
    [RequireComponent(typeof(Inventory))]
    public abstract partial class BasePlayer : NetworkBehaviour, IKillable
    {
        private bool HasFlag(PlayerFlags flag)
        {
            int value = (int) flag;
            return HasFlag(value);
        }

        private bool HasFlags(params PlayerFlags[] flags)
        {
            int value = 0;
            foreach (PlayerFlags fl in flags)
            {
                value |= (int) fl;
            }

            return HasFlag(value);
        }

        private bool HasFlag(int flag)
        {
            return (this.flags.Value & flag) == flag;
        }

        void Start()
        {
            SetupNestedComponents();
            var weapon = Inventory.CurrentWeapon;
            if (weapon != null)
            {
                SetHandTargets(weapon.RightHandTarget, weapon.LeftHandTarget);
            }

            gameObject.AddComponent<Jetpack>();
            Jetpack.FuelDuration = DefaultFuelDuration;

            cdManager = GetComponent<CooldownManager>();

            controller = GetComponent<CharacterController>();

            GameController.Singleton.AddPlayer(this);

            if (IsOwner)
            {
                GameObject playerCamera = GameObject.FindGameObjectWithTag("Player Camera");
                cameraController = playerCamera.GetComponent<CameraController>();
                UpdateCamera();

                Cursor.lockState = CursorLockMode.Locked;
            }
        }

        private void SetupNestedComponents()
        {
            Transform[] transforms = GetComponentsInChildren<Transform>();
            foreach (Transform t in transforms)
            {
                if (t.name == "weaponContainer")
                {
                    this.WeaponContainer = t;
                }
                else if (t.name == "ironman")
                    RigBuilder = t.GetComponent<RigBuilder>();
            }
        }

        /**
         * <summary>moves the player to the spawn</summary>
         */
        private void MoveToSpawn()
        {
            if (!IsOwner)
                return;
            var pos = GameController.Singleton.SpawnPoint;
            pos.y += 30;

            UpdatePositionRpc(pos);
        }

        void Awake()
        {
        }

        // Update is called once per frame
        private void Update()
        {
            if (IsOwner)
                UpdateClient();
            if (IsServer)
                UpdateServer();
        }

        public bool CanDamage(BasePlayer other)
        {
            return other.teamId.Value != teamId.Value;
        }


        private void UpdateServer()
        {
            if (IsShooting)
                this.Inventory.CurrentWeapon.Fire();
        }

        /**
         * <summary>the function is called in <see cref="Update"/> if instance is server</summary>
         */
        private void UpdateClient()
        {
            var _deltaTime = Time.deltaTime;

            handleDash(_deltaTime);

            focusedObject = GetClosestPickableObject(pickUpDistance);
            if (focusedObject != null)
            {
                Vector2 scalars = CameraController.Camera.WorldToViewportPoint(focusedObject.transform.position);
                GameController.Singleton.HUDController.ShowItemSelector(scalars);
            }
            else
            {
                GameController.Singleton.HUDController.HideItemSelector();
            }

            Vector3 res;

            if (!IsFlying)
            {
                var moveVector = Movement;

                float multiplier = movementSpeed;
                if (IsRunning)
                {
                    multiplier *= runningSpeedMultiplier;
                }

                res = Movement * multiplier;

                yVelocity += gravity * _deltaTime;
                if (IsGrounded && yVelocity < 0f)
                    yVelocity = 0;

                if (moveVector.y > 0)
                    Jump();

                res.y = yVelocity;
            }
            else
            {
                res = Jetpack.Velocity;
            }


            controller.Move(transform.TransformDirection(res) * _deltaTime);
            UpdateVelocityServerRpc(res);
            UpdateCamera();
        }

        /**
         * <summary>handler for jump function</summary>
         */
        private void Jump()
        {
            if (IsGrounded)
            {
                yVelocity = jumpForce;
            }
        }


        /**
         * <summary>
         *      Removes health from the player
         * </summary>
         * <param name="damage">int for amount of damage taken</param>
         */
        public bool TakeDamage(int damage)
        {
            if (damage >= CurrentHealth.Value)
            {
                OnKill();
                return false;
            }
            else
            {
                UpdateHealthServerRpc(CurrentHealth.Value - damage, this.OwnerClientId);
                return true;
            }
        }

        public void OnKill()
        {
            UpdateHealthServerRpc(0, this.OwnerClientId);
        }


        public void OnRespawn()
        {
            GameController.Singleton.deathScreen.SetActive(false);
            MoveToSpawn();
        }

        private void SetAim(bool aim)
        {
            if (aim == IsAiming)
                return;

            GameObject hud = Inventory.CurrentWeapon.AimingHUD;
            if (hud != null)
                hud.SetActive(aim);

            IsAiming = aim;

            if (aim)
            {
                IsRunning = false;
                cameraController.ChangeOffset(Inventory.CurrentWeapon.AimingOffset);
                UpdateCamera();
            }
            else
            {
                cameraController.ResetOffset();
                UpdateCamera();
            }
        }

        private void UpdateCamera()
        {
            cameraController.OnPlayerMove(camRotationAnchor, transform);
            UpdateAimVectorServerRpc(FireCastPoint, CameraController.Camera.transform.forward);
        }

        private void handleDash(float _deltaTime)
        {
            if (!IsDashing)
                return;
            if (dashStartedSince > dashDuration)
            {
                this.IsDashing = false;
            }
            else
            {
                dashStartedSince += _deltaTime;

                // a function : [0;1] => [0;1] with f(1) = 0
                // it acts as a smoothing function for the velocity change
                // Func<float, float> kernelFunction = x => Mathf.Exp(-5 * (float)Math.Pow(2 * x - 1, 2));

                controller.Move(_dashDirection * _deltaTime);
            }
        }

        /**
         * <summary>a method to get the <see cref="GameObject"/> in the line sight</summary>
         * <remarks></remarks>
         */
        public GameObject GetObjectInSight(float weaponRange)
        {
            RaycastHit hit;

            Vector3 castPoint, direction;
            if (IsServer)
                (castPoint, direction) = aimVector;
            else
                (castPoint, direction) = (FireCastPoint, cameraController.Camera.transform.forward);

            if (!Physics.Raycast(castPoint, direction, out hit, weaponRange))

                return null;

            return hit.collider.gameObject;
        }

        public void SetHandTargets(Transform right, Transform left)
        {
            if (RigBuilder == null)
                SetupNestedComponents();
            foreach (var tr in GetComponentsInChildren<Transform>())
            {
                if (tr.name == "RightHandIK")
                {
                    var ik = tr.GetComponent<TwoBoneIKConstraint>();
                    ik.data.target = right;
                    ik.weight = 1;
                    RigBuilder.Build();
                }
                else if (tr.name == "LeftHandIK")
                {
                    var ik = tr.GetComponent<TwoBoneIKConstraint>();
                    ik.data.target = left;
                    ik.weight = 1;
                    RigBuilder.Build();
                }
            }
        }
    }
}