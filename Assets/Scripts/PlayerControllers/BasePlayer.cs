using System;
using GameManagers;
using Items;
using model;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Interactions;
using UnityEngine.Purchasing;
using UnityEngine.SceneManagement;
using Weapons;

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
    [RequireComponent(typeof(CooldownManager))]
    public abstract class BasePlayer : NetworkBehaviour
    {
        public abstract string ClassName { get; }
        protected virtual float movementSpeed { get; } = 20f;

        /**
        * <value>the speed multiplier when running</value>
        */
        protected virtual float runningSpeedMultiplier { get; set; } = 2f;

        protected virtual float jumpForce { get; set; } = 5f;

        /**
        * <value>the mouse sensitivity</value>
        */
        [SerializeField] private float sensitivity = .1F;


        public bool IsRunning
        {
            get
                => this.HasFlag(PlayerFlags.MOVING) && this.HasFlag(PlayerFlags.RUNNING) &&
                   movement.Value.z >= Mathf.Abs(movement.Value.x);
            private set => UpdateFlagsServerRpc(PlayerFlags.RUNNING, value);
        }


        private CameraController cameraController;

        public CameraController CameraController => cameraController;

        protected CooldownManager cdManager;

        protected GameController gameController;
        
        // the world space point the camera will rotate around
        protected Vector3 camRotationAnchor
        {
            get
            {
                var cld = GetComponent<CapsuleCollider>();
                return transform.TransformPoint(cld.center + Vector3.up * (cld.height / 4));
            }
        }

        [SerializeField] private float gravity = -9.81f;

        /**
     * <value>stores the x,y,z inputs of the player</value>
     */
        private readonly NetworkVariable<Vector3> movement = new NetworkVariable<Vector3>(Vector3.zero);

        /**
         * <value>the yVelocity of the player</value>
         * <remarks>this velocity is only accessible to and used by the server</remarks>
         */
        private float yVelocity = 0f;

        /**
         * <value>the local velocity of the player</value>
         * <remarks>the velocity vector is only available on the server</remarks>
         */
        public Vector3 Velocity
        {
            get
            {
                if (!IsServer)
                {
                    throw new NullReferenceException("Velocity is only available on the server");
                }

                if (IsFlying)
                {
                    return Jetpack.Velocity;
                }
                else
                {
                    float multiplier = movementSpeed;
                    if (IsRunning)
                    {
                        multiplier *= runningSpeedMultiplier;
                    }

                    Vector3 res = Movement * multiplier;
                    res.y = yVelocity;
                    return res;
                }
            }
        }


        public Vector3 Movement => movement.Value;

        private CharacterController controller;


        public bool IsGrounded
        {
            get
            {
                RaycastHit hit;
                Vector3 initPos = transform.position + controller.center;
                if (Physics.SphereCast(initPos, controller.height / 2, Vector3.down, out hit, 1))
                {
                    return hit.distance <= 0.2f && hit.collider.CompareTag("Ground");
                }


                return false;
            }
        }

        public bool IsFlying
        {
            get => HasFlag(PlayerFlags.FLYING) && !IsGrounded;
            set => UpdateFlagsServerRpc(PlayerFlags.FLYING, value);
        }


        public bool IsShooting
        {
            get => HasFlag(PlayerFlags.SHOOTING);
            set => UpdateFlagsServerRpc(PlayerFlags.SHOOTING, value);
        }

        public bool IsAiming
        {
            get => HasFlag(PlayerFlags.AIMING);
            private set => UpdateFlagsServerRpc(PlayerFlags.AIMING, value);
        }


        protected Inventory inventory;

        public Inventory Inventory => inventory;
        public Jetpack Jetpack => inventory.Jetpack;

        /** <value>the duration of the dash in seconds</value> */
        protected virtual float dashDuration { get; set; } = 0.3F;

        protected virtual float dashForce { get; set; } = 30f;
        
        public int MaxHealth => maxHealth;


        protected abstract int maxHealth { get; }

        /** <value>current player health</value> */
        private NetworkVariable<int> CurrentHealth { get; } = new NetworkVariable<int>(1);

        private NetworkVariable<int> teamId = new NetworkVariable<int>(-1);

        private NetworkVariable<int> flags = new NetworkVariable<int>(0);

        public int CurrentHealthValue => CurrentHealth.Value;

        /**
        * <value>the time in seconds since the dash has been called</value>
        */
        private float dashStartedSince = -1f;

        public bool IsDashing
        {
            get => HasFlag(PlayerFlags.DASHING);
            set => UpdateFlagsServerRpc(PlayerFlags.DASHING, value);
        }

        // getters for respectively the default dash cooldown and the time since last dash
        public float DashCooldown => cdManager.DashCooldown;
        public float DashedSince => cdManager.DashedSince;

        private Vector3 _dashDirection = Vector3.zero;

        // default value for fuel duration
        public float DefaultFuelDuration { get; } = 20f;

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
            GameObject gameManager = GameObject.FindGameObjectWithTag("GameController");
            gameController = gameManager.GetComponent<GameController>();

            this.inventory = new Inventory(this);

            GameObject autoWeaponPrefab = gameController.WeaponPrefabs.Find(go => go.name == "TestRiflePrefab");
            this.inventory.AddWeapon(Instantiate(autoWeaponPrefab).GetComponent<BaseWeapon>());

            GameObject gunWeaponPrefab = gameController.WeaponPrefabs.Find(go => go.name == "TestGunPrefab");
            this.inventory.AddWeapon(Instantiate(gunWeaponPrefab).GetComponent<BaseWeapon>());

            inventory.Jetpack = gameObject.AddComponent<Jetpack>();
            inventory.Jetpack.FuelDuration = DefaultFuelDuration;

            GameObject playerCamera = GameObject.FindGameObjectWithTag("Player Camera");
            cameraController = playerCamera.GetComponent<CameraController>();
            UpdateCamera();


            cdManager = gameObject.AddComponent<CooldownManager>();

            UpdateHealthServerRpc(maxHealth, OwnerClientId);
            controller = gameObject.GetComponent<CharacterController>();

            if (IsOwner)
            {
                gameController.BindPlayer(this);

                UpdateTeamServerRpc(gameController.Parameters.IsReady ? gameController.Parameters.TeamId : 0);
            }
        }

        void Awake()
        {
        }

        // Update is called once per frame
        private void Update()
        {
            if (IsClient)
                UpdateClient();
            if (IsServer)
                UpdateServer();
        }

        public bool CanDamage(BasePlayer other)
        {
            return other.teamId.Value != teamId.Value;
        }

        /**
         * <summary>the function is called in <see cref="Update"/> if instance is server</summary>
         */
        private void UpdateServer()
        {
            var _deltaTime = Time.deltaTime;

            handleDash(_deltaTime);
            var moveVector = movement.Value;
            if (!IsFlying)
            {
                yVelocity += gravity * _deltaTime;
                if (IsGrounded && yVelocity < 0f)
                    yVelocity = 0;

                if (moveVector.y > 0)
                    Jump();
            }

            controller.Move(transform.TransformDirection(this.Velocity) * _deltaTime);
        }

        /**
         * <summary>the function is called in <see cref="FixedUpdate"/> if instance is a client</summary>
         */
        private void UpdateClient()
        {
            if (!IsOwner)
                return;

            UpdateCamera();

            cameraController.OnPlayerMove(camRotationAnchor, transform);

            if (IsShooting)
                this.inventory.CurrentWeapon.Fire();
        }

        /**
         * <summary>
         *      Called when the move event is triggered within unity
         * </summary>
         * <param name="ctx">the <see cref="InputAction.CallbackContext"/> giving the move axis values </param>
         */
        public void OnMove(InputAction.CallbackContext ctx)
        {
            if (!IsOwner)
                return;

            if (ctx.performed)
            {
                Vector2 direction = ctx.ReadValue<Vector2>();
                Vector3 moveVector = new Vector3(direction.x, Movement.y, direction.y);

                UpdateMovementServerRpc(moveVector);
            }
            else
            {
                var moveVector = new Vector3(0, Movement.y, 0);
                UpdateMovementServerRpc(moveVector);
            }
        }


        /**
         * <summary>
         *      Called when the rotation event is triggered within unity
         * </summary>
         * <param name="ctx">the <see cref="InputAction.CallbackContext"/> giving the rotation delta </param>
         */
        public void OnRotation(InputAction.CallbackContext ctx)
        {
            if (!IsOwner || cameraController == null)
                return;
            Vector2 rotation = ctx.ReadValue<Vector2>();
            UpdateRotationServerRpc(Vector3.up, rotation.x * sensitivity);

            cameraController.AddedAngle -= rotation.y * sensitivity;
        }

        /**
         * <summary>
         *      Called when the jump event is triggered within unity
         * </summary>
         */
        public void OnJump(InputAction.CallbackContext ctx)
        {
            if (!IsOwner)
                return;


            var moveVector = Movement;

            if (ctx.interaction is MultiTapInteraction && ctx.performed)
            {
                moveVector.y = 0;
                this.Jetpack.IsFlying = !this.Jetpack.IsFlying;
            }
            else
            {
                moveVector.y = (ctx.started || ctx.performed) && !ctx.canceled ? 1 : 0;
            }

            UpdateMovementServerRpc(moveVector);
        }

        /**
         * <summary>Server-Side handler for jump function</summary>
         */
        private void Jump()
        {
            if (IsGrounded)
            {
                yVelocity = jumpForce;
            }
        }

        public void OnLowerJetpack(InputAction.CallbackContext ctx)
        {
            if (!IsOwner)
                return;

            var currentMovement = Movement;
            if (ctx.performed)
            {
                currentMovement.y = -1;
            }
            else if (ctx.canceled)
            {
                currentMovement.y = 0;
            }

            // ServerRpc methods operate not changes if values are unchanged 
            // if(currentMovement != Movement)
            UpdateMovementServerRpc(currentMovement);
        }

        public void OnReload(InputAction.CallbackContext _)
        {
            if (!IsOwner)
                return;
            BaseWeapon weapon = inventory.CurrentWeapon;
            if (weapon.CurrentAmmo < weapon.MaxAmmo)
            {
                inventory.CurrentWeapon.Reload();
                SetAim(false);
            }
        }

        /**
         * <summary>called when run button is toggled</summary>
         */
        public void OnRun(InputAction.CallbackContext ctx)
        {
            if (!IsOwner)
                return;


            UpdateFlagsServerRpc(PlayerFlags.RUNNING, !IsAiming && ctx.performed);
        }

        public void OnDash(InputAction.CallbackContext ctx)
        {
            if (!IsOwner || !cdManager.RequestDash())
                return;

            IsDashing = true;
            SetAim(false);
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
                UpdateHealthServerRpc(0, this.OwnerClientId);
                return false;
            }
            else
            {
                UpdateHealthServerRpc(CurrentHealth.Value - damage, this.OwnerClientId);
                return true;
            }
        }

        public void OnFire(InputAction.CallbackContext ctx)
        {
            if (!IsOwner)
                return;
            IsShooting = ctx.ReadValueAsButton();
        }

        public void OnRespawn()
        {
            this.UpdatePositionServerRpc(new Vector3(0f,0f,0f));
            
            GameObject autoWeaponPrefab = gameController.WeaponPrefabs.Find(go => go.name == "TestAutoPrefab");
            this.inventory.AddWeapon(Instantiate(autoWeaponPrefab).GetComponent<BaseWeapon>());

            GameObject gunWeaponPrefab = gameController.WeaponPrefabs.Find(go => go.name == "TestGunPrefab");
            this.inventory.AddWeapon(Instantiate(gunWeaponPrefab).GetComponent<BaseWeapon>());
            
            foreach (BaseItem item in Inventory.ItemsList)
            {
                Inventory.RemoveItem(item);
            }
            
            gameController.deathScreen.SetActive(false);
        }
        
        public void OnWeaponSwitch(InputAction.CallbackContext ctx)
        {
            if (!IsOwner || IsAiming)
                return;

            // mouse wheel control
            if (ctx.action.type == InputActionType.PassThrough)
            {
                float value = ctx.ReadValue<float>();
                if (value > 0)
                    inventory.NextWeapon();
                else if (value < 0)
                    inventory.PreviousWeapon();
            }
            else // 1,2,3 control
            {
                // TODO : implement 1,2,3 weapon switch control
            }
        }

        public void OnAim(InputAction.CallbackContext ctx)
        {
            if (!IsOwner)
                return;

            SetAim(!inventory.CurrentWeapon.IsReloading && ctx.performed);
        }

        private void SetAim(bool aim)
        {
            if (aim == IsAiming)
                return;

            GameObject hud = inventory.CurrentWeapon.AimingHUD;
            if (hud != null)
                hud.SetActive(aim);

            IsAiming = aim;

            if (aim)
            {
                IsRunning = false;
                cameraController.ChangeOffset(inventory.CurrentWeapon.AimingOffset);
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
         */
        public GameObject GetObjectInSight(float weaponRange)
        {
            Vector2 crosshairPosition = new Vector2(0.5f, 0.5f);

            Vector3 origin = cameraController.Camera.ViewportToWorldPoint(crosshairPosition);

            RaycastHit hit;

            if (!Physics.Raycast(origin, cameraController.Camera.transform.forward, out hit, weaponRange))

                return null;

            return hit.collider.gameObject;
        }


        [ServerRpc(RequireOwnership = false)]
        public void UpdateHealthServerRpc(int newHealth, ulong playerId)
        {
            BasePlayer damagedPlayer = NetworkManager.Singleton.ConnectedClients[playerId].PlayerObject
                .GetComponent<BasePlayer>();
            
            damagedPlayer.CurrentHealth.Value = newHealth;
        }

        [ServerRpc]
        public void UpdateMovementServerRpc(Vector3 movement)
        {
            UpdateFlagsServerRpc(PlayerFlags.MOVING, movement.magnitude > 0);
            this.movement.Value = movement;
        }
        
        [ServerRpc(RequireOwnership = false)]
        public void UpdatePositionServerRpc(Vector3 position)
        {
            transform.position = position;
        }

        [ServerRpc]
        public void UpdateRotationServerRpc(Vector3 direction, float angle)
        {
            transform.Rotate(direction, angle);
        }

        // ownership ain't required since server has to be able to change the flags
        [ServerRpc(RequireOwnership = false)]
        private void UpdateFlagsServerRpc(PlayerFlags flag, bool add = true)
        {
            int value = flags.Value;

            if (add)
            {
                value |= (int) flag;
            }
            else
            {
                value &= (int) ~flag;
            }

            this.flags.Value = value;

            // flag specific settings
            switch (flag)
            {
                case PlayerFlags.DASHING:
                    dashStartedSince = 0;
                    Vector3 moveVector = Movement;
                    if (moveVector == Vector3.zero)
                        moveVector = Vector3.forward;
                    _dashDirection = transform.TransformDirection(moveVector) * dashForce;
                    break;
                case PlayerFlags.FLYING:
                    yVelocity = 0;
                    break;
            }
        }

        [ServerRpc]
        private void UpdateTeamServerRpc(int teamId)
        {
            this.teamId.Value = teamId;
        }
    }
}