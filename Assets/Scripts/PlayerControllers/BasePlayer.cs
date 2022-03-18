using System;
using System.Collections.Generic;
using GameManagers;
using Items;
using Minions;
using model;
using Unity.Netcode;
using Unity.Netcode.Samples;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Interactions;
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
    [RequireComponent(typeof(ClientNetworkTransform))]
    [RequireComponent(typeof(CooldownManager))]
    public abstract class BasePlayer : NetworkBehaviour, IKillable
    {
        public abstract string ClassName { get; }
        protected virtual float movementSpeed { get; } = 5f;

        /**
        * <value>the speed multiplier when running</value>
        */
        protected virtual float runningSpeedMultiplier { get; } = 2f;

        protected virtual float jumpForce { get; } = 5f;

        /**
        * <value>the mouse sensitivity</value>
        */
        [SerializeField] private float sensitivity = .1F;


        public bool IsRunning
        {
            get
                => this.HasFlag(PlayerFlags.MOVING) && this.HasFlag(PlayerFlags.RUNNING) &&
                   velocity.Value.z >= Mathf.Abs(velocity.Value.x);
            private set => UpdateFlagsServerRpc(PlayerFlags.RUNNING, value);
        }

        private Transform[] weapons;
        private Transform[] weaponRends;


        private CameraController cameraController;

        public CameraController CameraController => cameraController;

        protected CooldownManager cdManager;

        // the world space point the camera will rotate around
        protected Vector3 camRotationAnchor
        {
            get
            {
                var cld = GetComponent<CapsuleCollider>();
                return transform.TransformPoint(cld.center + Vector3.up * (cld.height / 4));
            }
        }

        protected Vector3 FireCastPoint
        {
            get
            {
                var cld = GetComponent<CapsuleCollider>();
                Vector2 crosshairPosition = new Vector2(0.5f, 0.5f);

                Vector3 origin = cameraController.Camera.ViewportToWorldPoint(crosshairPosition);
                origin += cameraController.gameObject.transform.forward * cameraController.Offset.magnitude;
                return origin;
            }
        }

        [SerializeField] private float gravity = -9.81f;

        /**
     * <value>stores the x,y,z inputs of the player</value>
     */
        // private readonly NetworkVariable<Vector3> movement = new NetworkVariable<Vector3>(Vector3.zero);

        public Vector3 Movement { get; private set; } = Vector3.zero;

        /**
         * <value>the yVelocity of the player</value>
         * <remarks>this velocity is only accessible to and used by the server</remarks>
         */
        private float yVelocity = 0f;

        private NetworkVariable<Vector3> velocity = new NetworkVariable<Vector3>(Vector3.zero);

        /**
         * <value>the local velocity of the player</value>
         * <remarks>the velocity vector is only available on the server</remarks>
         */
        public Vector3 Velocity => IsFlying ? Jetpack.Velocity : velocity.Value;

        /**
         * <value>the projected position on the ground when not grounded</value>
         */
        public Vector3 GroundPosition
        {
            get
            {
                Vector3 res = transform.position;
                RaycastHit hit;
                if (!IsGrounded &&
                    Physics.Raycast(transform.position + controller.center - controller.height / 2 * Vector3.up,
                        Vector3.down, out hit) && hit.collider.CompareTag("Ground"))
                {
                    res.y -= hit.distance - controller.height / 2;
                }

                return res;
            }
        }


        private CharacterController controller;


        public bool IsGrounded
        {
            get
            {
                RaycastHit hit;
                Vector3 initPos = transform.position + controller.center;
                if (Physics.Raycast(initPos, Vector3.down, out hit, controller.height))
                {
                    return hit.collider.CompareTag("Ground");
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

        protected Vector3 spawnPoint = Vector3.zero;

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

        public int TeamId => teamId.Value;

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
        public float DefaultFuelDuration { get; } = 10f;

        [SerializeField] private int maxMinions = 2;

        public List<BaseMinion> Minions => GameController.Singleton.Minions.FindAll(m => m.OwnerId == OwnerClientId);

        private int strategy = 0;
        public int Strategy => strategy;

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
            this.inventory = new Inventory(this);

            GameObject autoWeaponPrefab =
                GameController.Singleton.WeaponPrefabs.Find(go => go.name == "TestAutoPrefab");
            this.inventory.AddWeapon(Instantiate(autoWeaponPrefab).GetComponent<BaseWeapon>());

            GameObject gunWeaponPrefab = GameController.Singleton.WeaponPrefabs.Find(go => go.name == "TestGunPrefab");
            this.inventory.AddWeapon(Instantiate(gunWeaponPrefab).GetComponent<BaseWeapon>());

            inventory.Jetpack = gameObject.AddComponent<Jetpack>();
            inventory.Jetpack.FuelDuration = DefaultFuelDuration;


            cdManager = gameObject.AddComponent<CooldownManager>();

            controller = gameObject.GetComponent<CharacterController>();

            if (IsOwner)
            {
                UpdateHealthServerRpc(maxHealth, OwnerClientId);
                GameController.Singleton.BindPlayer(this);

                int teamId = GameController.Singleton.Parameters.IsReady
                    ? GameController.Singleton.Parameters.TeamId
                    : 0;
                UpdateTeamServerRpc(teamId);

                if (teamId == 1)
                {
                    GameObject obj = GameObject.FindGameObjectWithTag("SpawnPoint2");
                    spawnPoint = obj.transform.position;
                }
                else
                {
                    GameObject obj = GameObject.FindGameObjectWithTag("SpawnPoint1");
                    spawnPoint = obj.transform.position;
                }

                Debug.Log($"spawning at {spawnPoint}");
                spawnPoint -= controller.height * Vector3.up;

                UpdatePositionRpc(spawnPoint);
                GameObject playerCamera = GameObject.FindGameObjectWithTag("Player Camera");
                cameraController = playerCamera.GetComponent<CameraController>();
                UpdateCamera();


                Cursor.lockState = CursorLockMode.Locked;
            }
            else
            {
                GameController.Singleton.AddClientPlayer(this);
            }

            weapons = GetComponentsInChildren<Transform>();
            foreach (Transform weaponModel in weapons)
            {
                if (weaponModel.CompareTag("Weapon"))
                {
                    weaponModel.GetComponent<MeshRenderer>().enabled =
                        String.Equals(weaponModel.name, Inventory.CurrentWeapon.Name);
                    weaponRends = weaponModel.GetComponentsInChildren<Transform>();
                    foreach (Transform tran in weaponRends)
                    {
                        if (tran.GetComponent<MeshRenderer>() != null)
                        {
                            tran.GetComponent<MeshRenderer>().enabled =
                                String.Equals(weaponModel.name, Inventory.CurrentWeapon.Name);
                        }
                    }
                }
            }
        }

        void Awake()
        {
        }

        // Update is called once per frame
        private void Update()
        {
            if (IsOwner)
            {
                UpdateServer();
                UpdateClient();
            }
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

                UpdateMovement(moveVector);
            }
            else
            {
                var moveVector = new Vector3(0, Movement.y, 0);
                UpdateMovement(moveVector);
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
            UpdateRotationRpc(Vector3.up, rotation.x * sensitivity);

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
                yVelocity = 0;
                this.Jetpack.IsFlying = !this.Jetpack.IsFlying;
            }
            else
            {
                moveVector.y = (ctx.started || ctx.performed) && !ctx.canceled ? 1 : 0;
            }

            UpdateMovement(moveVector);
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
            UpdateMovement(currentMovement);
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
            dashStartedSince = 0;
            Vector3 moveVector = Movement;
            if (moveVector == Vector3.zero)
                moveVector = Vector3.forward;
            _dashDirection = transform.TransformDirection(moveVector) * dashForce;
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

        public void OnFire(InputAction.CallbackContext ctx)
        {
            if (!IsOwner)
                return;
            IsShooting = ctx.ReadValueAsButton();
        }

        public void OnRespawn()
        {
            // respawn location
            this.UpdatePositionRpc(spawnPoint);

            GameObject autoWeaponPrefab =
                GameController.Singleton.WeaponPrefabs.Find(go => go.name == "TestAutoPrefab");
            this.inventory.AddWeapon(Instantiate(autoWeaponPrefab).GetComponent<BaseWeapon>());

            GameObject gunWeaponPrefab = GameController.Singleton.WeaponPrefabs.Find(go => go.name == "TestGunPrefab");
            this.inventory.AddWeapon(Instantiate(gunWeaponPrefab).GetComponent<BaseWeapon>());

            foreach (BaseItem item in Inventory.ItemsList)
            {
                Inventory.RemoveItem(item);
            }

            GameController.Singleton.deathScreen.SetActive(false);
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
                    inventory.PreviousWeapon();
                else if (value < 0)
                    inventory.NextWeapon();
                //change currentWeaponModel
                weapons = GetComponentsInChildren<Transform>();
                foreach (Transform weaponModel in weapons)
                {
                    if (weaponModel.CompareTag("Weapon"))
                    {
                        weaponModel.GetComponent<MeshRenderer>().enabled =
                            String.Equals(weaponModel.name, Inventory.CurrentWeapon.Name);
                        weaponRends = weaponModel.GetComponentsInChildren<Transform>();
                        foreach (Transform tran in weaponRends)
                        {
                            if (tran.GetComponent<MeshRenderer>() != null)
                            {
                                tran.GetComponent<MeshRenderer>().enabled =
                                    String.Equals(weaponModel.name, Inventory.CurrentWeapon.Name);
                            }
                        }
                    }
                }
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
            RaycastHit hit;


            if (!Physics.Raycast(FireCastPoint, cameraController.Camera.transform.forward, out hit, weaponRange))

                return null;

            return hit.collider.gameObject;
        }

        public void OnChangeStrategy(InputAction.CallbackContext ctx)
        {
            if (!IsOwner || !ctx.started)
                return;
            strategy = (strategy + 1) % 3;
        }
        public void OnSpawnMinion(InputAction.CallbackContext ctx)
        {
            if (!IsOwner || !ctx.started)
                return;

            var tr = this.transform;
            SpawnMinionServerRpc((IMinion.Strategy) strategy, tr.position - tr.forward, tr.rotation);
        }

        /**
         * <summary>spawns a minion bound to the player</summary>
         */
        [ServerRpc]
        protected void SpawnMinionServerRpc(IMinion.Strategy strat, Vector3 position, Quaternion rotation)
        {
            if (Minions.Count >= maxMinions || !cdManager.RequestSpawnMinion())
                return;

            GameObject instance = Instantiate(GameController.Singleton.MinionPrefab, position, rotation);
            instance.GetComponent<NetworkObject>().Spawn();
            BaseMinion minion = instance.GetComponent<BaseMinion>();
            minion.BindOwnerServerRpc(this.OwnerClientId, strat);
        }


        [ServerRpc(RequireOwnership = false)]
        public void UpdateHealthServerRpc(int newHealth, ulong playerId)
        {
            BasePlayer damagedPlayer = NetworkManager.Singleton.ConnectedClients[playerId].PlayerObject
                .GetComponent<BasePlayer>();

            damagedPlayer.CurrentHealth.Value = newHealth;
        }

        //[ServerRpc]
        //public void UpdateMovementServerRpc(Vector3 movement)
        public void UpdateMovement(Vector3 movement)
        {
            UpdateFlagsServerRpc(PlayerFlags.MOVING, movement.magnitude > 0);
            this.Movement = movement;
        }

        [ServerRpc]
        private void UpdateVelocityServerRpc(Vector3 velocity)
        {
            this.velocity.Value = velocity;
        }

        //[ServerRpc]
        public void UpdatePositionRpc(Vector3 position)
        {
            controller.Move(position);
        }

        //[ServerRpc]
        public void UpdateRotationRpc(Vector3 direction, float angle)
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

                    break;
                case PlayerFlags.FLYING:
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