using System;
using GameManagers;
using model;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Interactions;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using Weapons;

namespace PlayerControllers
{
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


        private bool isRunning;

        public bool IsRunning => isRunning && movement.Value.z >= Mathf.Abs(movement.Value.x);


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
     * <value>the current movement.Value requested to be made</value>
         * <remarks>it stores in y the vertical velocity</remarks>
     */
        private readonly NetworkVariable<Vector3> movement = new NetworkVariable<Vector3>(Vector3.zero);

        /**
         * <value>a aching value for <see cref="movement"/></value>
         */
        private Vector3 _movement = Vector3.zero;

        public Vector3 Movement => movement.Value;

        /** <value>the y direction for the <see cref="Jetpack"/>, -1 => down, 1 => up, 0 => unchanged </value> */
        private int yDirection = 0;

        private CharacterController controller;

        public Rigidbody RigidBody;


        public bool IsGrounded
        {
            get
            {
                RaycastHit hit;
                Vector3 initPos = transform.position + controller.center;
                if (Physics.SphereCast(initPos, controller.height / 2, Vector3.down, out hit, 10))
                {
                    return hit.distance <= 0.2f && hit.collider.gameObject.CompareTag("Ground");
                }


                return false;
            }
        }


        private bool isShooting = false;
        public bool IsShooting => isShooting;

        private Vector3 dashDirection;


        protected Inventory inventory;

        public Inventory Inventory => inventory;
        public Jetpack Jetpack => inventory.Jetpack;

        [SerializeField] protected GameObject deathScreenPrefab;
        private GameObject deathScreen;

        /** <value>the duration of the dash in seconds</value> */
        protected virtual float dashDuration { get; set; } = 0.3F;

        protected virtual float dashForce { get; set; } = 80f;

        public int MaxHealth => maxHealth;


        protected abstract int maxHealth { get; }

        /** <value>current player health</value> */
        protected NetworkVariable<int> CurrentHealth { get; } = new NetworkVariable<int>(1);

        public int CurrentHealthValue => CurrentHealth.Value;

        /**
        * <value>the time in seconds since the dash has been called</value>
        */
        private float dashStartedSince = -1f;

        public bool IsDashing => dashStartedSince > 0 && dashStartedSince <= dashDuration;

        // getters for respectively the default dash cooldown and the time since last dash
        public float DashCooldown => cdManager.DashCooldown;
        public float DashedSince => cdManager.DashedSince;

        // default value for fuel duration
        public float DefaultFuelDuration { get; } = 20f;

        void Start()
        {
            GameObject gameManager = GameObject.FindGameObjectWithTag("GameController");
            gameController = gameManager.GetComponent<GameController>();

            this.inventory = new Inventory(this);

            GameObject testWeaponPrefab = gameController.WeaponPrefabs.Find(go => go.name == "TestAutoPrefab");
            this.inventory.AddWeapon(Instantiate(testWeaponPrefab).GetComponent<BaseWeapon>());

            // TODO : uncomment
            // this.inventory.Jetpack = gameObject.AddComponent<Jetpack>();
            // this.inventory.Jetpack.FuelDuration = DefaultFuelDuration;

            GameObject playerCamera = GameObject.FindGameObjectWithTag("Player Camera");
            cameraController = playerCamera.GetComponent<CameraController>();
            cameraController.OnPlayerMove(camRotationAnchor, transform);

            if (IsLocalPlayer)
                gameController.BindPlayer(this);

            cdManager = gameObject.AddComponent<CooldownManager>();

            UpdateHealthServerRpc(maxHealth, OwnerClientId);
            controller = gameObject.GetComponent<CharacterController>();

            deathScreen = Instantiate(deathScreenPrefab);
            deathScreen.name = deathScreenPrefab.name;
            deathScreen.GetComponent<Canvas>().worldCamera = Camera.current;
            deathScreen.SetActive(false);

            movement.OnValueChanged += OnMovementChanged;
        }

        void OnMovementChanged(Vector3 oldVal, Vector3 newVal)
        {
            this._movement = newVal;
        }

        void Awake()
        {
        }

        private void ServerFixedUpdate()
        {
            handleDash();
            if (movement.Value.magnitude > 0f)
            {
                Vector3 moveVector = Vector3.ClampMagnitude(movement.Value, 1f);
                moveVector = transform.TransformVector(moveVector);

                if (true || !this.inventory.Jetpack.IsFlying)
                {
                    var speed = movementSpeed * (IsRunning ? runningSpeedMultiplier : 1F);
                    controller.SimpleMove(moveVector * speed);
                }
            }
            else if (IsDashing)
            {
            }
        }

        // Update is called once per frame
        private void Update()
        {
            if (IsServer)
                UpdateServer();
            if (IsClient)
                UpdateClient();
        }

        /**
         * <summary>the function is called in <see cref="FixedUpdate"/> if instance is server</summary>
         */
        private void UpdateServer()
        {
            handleDash();
            var velocity = _movement;
            velocity.y += gravity * Time.deltaTime;
            if (IsGrounded && velocity.y < 0f)
                velocity.y = 0;
            
            Debug.Log($"velocity is {velocity}");
            this.movement.Value = velocity;


            controller.Move(transform.TransformDirection(movement.Value) * Time.deltaTime);
        }

        /**
         * <summary>the function is called in <see cref="FixedUpdate"/> if instance is a client</summary>
         */
        private void UpdateClient()
        {
            if (IsLocalPlayer)
            {
                cameraController.OnPlayerMove(camRotationAnchor, transform);
                if (CurrentHealth.Value == 0)
                {
                    SceneManager.LoadScene("DeathScreen");
                }
            }
        }

        /**
         * <summary>
         *      Called when the move event is triggered within unity
         * </summary>
         * <param name="ctx">the <see cref="InputAction.CallbackContext"/> giving the move axis values </param>
         */
        public void OnMove(InputAction.CallbackContext ctx)
        {
            if (!IsLocalPlayer)
                return;

            if (ctx.performed)
            {
                Vector2 direction = ctx.ReadValue<Vector2>() * movementSpeed;
                if (isRunning)
                    direction *= runningSpeedMultiplier;

                UpdateMovementServerRpc(new Vector3(direction.x, _movement.y, direction.y));
            }
            else
            {
                UpdateMovementServerRpc(new Vector3(0, _movement.y, 0));
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
            if (!IsLocalPlayer || cameraController == null)
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
            if (!IsLocalPlayer)
                return;

            /*if (this.Jetpack.IsFlying)
            {
                yDirection = ctx.ReadValueAsButton() ? 1 : 0;
            }

            if (ctx.interaction is MultiTapInteraction && ctx.performed)
            {
                this.yDirection = 0;
                this.Jetpack.IsFlying = !this.Jetpack.IsFlying;
            }
            else if (!this.Jetpack.IsFlying)
            {
                Jump();
            }*/
            Jump();
        }

        private void Jump()
        {
            if (IsGrounded)
                UpdateMovementServerRpc(new Vector3(_movement.x, jumpForce, _movement.y));
        }

        public void OnLowerJetpack(InputAction.CallbackContext ctx)
        {
            if (!IsLocalPlayer)
                return;

            if (ctx.performed)
            {
                yDirection = -1;
            }
            else if (ctx.canceled)
            {
                yDirection = 0;
            }
        }

        /**
         * <summary>called when run button is toggled</summary>
         */
        public void OnRun(InputAction.CallbackContext ctx)
        {
            if (!IsLocalPlayer)
                return;

            isRunning = ctx.performed;
        }

        public void OnDash(InputAction.CallbackContext ctx)
        {
            if (!IsLocalPlayer || !cdManager.RequestDash())
                return;

            dashStartedSince = Time.fixedDeltaTime;

            dashDirection = Vector3.ClampMagnitude(_movement + yDirection * Vector3.up, 1f);

            if (dashDirection == Vector3.zero)
            {
                dashDirection = Vector3.forward;
            }

            dashDirection = transform.TransformDirection(dashDirection);
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
            isShooting = ctx.ReadValueAsButton();
        }


        private void handleDash()
        {
            if (dashDirection == Vector3.zero)
                return;

            if (dashStartedSince > dashDuration)
            {
                dashStartedSince = 0;
                dashDirection = Vector3.zero;
            }
            else if (IsDashing)
            {
                dashStartedSince += Time.deltaTime;

                // a function : [0;1] => [0;1] with f(1) = 0
                // it acts as a smoothing function for the velocity change
                Func<float, float> kernelFunction = x => Mathf.Exp(-5 * (float) Math.Pow(2 * x - 1, 2));

                Vector3 velocity = dashDirection * (kernelFunction(dashStartedSince / dashDuration) * dashForce) *
                                   movementSpeed;
                controller.SimpleMove(velocity);
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
        public void UpdateMovementServerRpc(Vector3 _movement)
        {
            Debug.Log($"new movement: {_movement}");
            movement.Value = _movement;
        }

        [ServerRpc]
        public void UpdateRotationServerRpc(Vector3 direction, float angle)
        {
            transform.Rotate(direction, angle);
        }
    }
}