using System;
using Unity.Netcode;
using Unity.Netcode.Samples;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Interactions;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

/**
 * <summary>
 *      The controller class for any player spawned in the game
 * </summary>
 */
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(NetworkObject))]
[RequireComponent(typeof(ClientNetworkTransform))]
[RequireComponent(typeof(CooldownManager))]
public class PlayerController : NetworkBehaviour
{
    // Constants to be set by unity
    [SerializeField] private float movementSpeed = 10F;

    /**
     * <value>the speed multiplier when running</value>
     */
    [SerializeField] private float runningSpeedMultiplier = 2f;

    [SerializeField] private float jumpForce = 5f;

    /**
     * <value>the mouse sensitivity</value>
     */
    [SerializeField] private float sensitivity = .1F;


    protected Jetpack jetpack;

    public Jetpack Jetpack => jetpack;

    private bool isRunning;

    public bool IsRunning => isRunning;

    protected CameraController cameraController;
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

    /**
     * <value>the current movement requested to be made</value>
     */
    private Vector3 movement = Vector3.zero;

    /**
     * <value>the y direction for the <see cref="jetpack"/>, -1 => down, 1 => up, 0 => unchanged </value>
     */
    private int yDirection = 0;

    protected Rigidbody rigidBody;
    public Rigidbody RigidBody => rigidBody;

    protected bool isGrounded;

    public bool IsGrounded => isGrounded;


    protected bool isShooting = false;
    public bool IsShooting => isShooting;

    private Vector3 dashDirection;

    /**
     * <value>the duration of the dash in seconds</value>
     */
    [SerializeField] protected float dashDuration = 0.3F;

    [SerializeField] protected float dashForce = 80f;

    public int MaxHealth => maxHealth;

    /**
     * <value>current player health</value>
     */
    [SerializeField] protected int maxHealth = 100;

    [SerializeField] protected NetworkVariable<int> currentHealth { get; } = new NetworkVariable<int>(1);

    public int GetCurrentHealth()
    {
        return currentHealth.Value;
    }

    /**
     * <value>the time in seconds since the dash has been called</value>
     */
    private float dashStartedSince = -1f;

    public bool IsDashing => dashStartedSince > 0 && dashStartedSince <= dashDuration;

    void Start()
    {
        this.jetpack = gameObject.AddComponent<Jetpack>();
        this.jetpack.Player = this;

        this.jetpack.FuelDuration = 20f;

        GameObject playerCamera = GameObject.FindGameObjectWithTag("Player Camera");
        cameraController = playerCamera.GetComponent<CameraController>();
        cameraController.OnPlayerMove(camRotationAnchor, transform);

        GameObject gameManager = GameObject.FindGameObjectWithTag("GameController");
        gameController = gameManager.GetComponent<GameController>();

        if (IsLocalPlayer)
            gameController.BindPlayer(this);

        cdManager = gameObject.AddComponent<CooldownManager>();

        UpdateHealthServerRpc(maxHealth, OwnerClientId);
    }

    void Awake()
    {
        rigidBody = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        if (!IsLocalPlayer)
            return;

        var playerTransform = transform;

        // dash has to be handled before movement
        handleDash();

        if (movement != Vector3.zero && !IsDashing)
        {
            Vector3 moveVector = Vector3.ClampMagnitude(movement, 1f);
            moveVector = transform.TransformVector(moveVector);

            var velocity = rigidBody.velocity;

            if (!this.jetpack.IsFlying)
            {
                var speed = movementSpeed * (isRunning ? runningSpeedMultiplier : 1F);


                Vector3 deltaVelocity = new Vector3(velocity.x, 0f, velocity.z);

                rigidBody.AddForce(moveVector * speed - deltaVelocity, ForceMode.VelocityChange);
            }
        }

        if (this.jetpack.IsFlying)
        {
            Vector3 moveVector = Vector3.ClampMagnitude(movement, 1f);
            moveVector = Vector3.ClampMagnitude(moveVector + yDirection * Vector3.up, 1f);
            moveVector = transform.TransformVector(moveVector);

            this.jetpack.Direction = moveVector;
        }

        if (isShooting)
        {
            Shoot();
        }


        // forces the capsule to stand up
        // playerTransform.eulerAngles = new Vector3(0, playerTransform.eulerAngles.y, 0);
    }

    // Update is called once per frame
    private void Update()
    {
        if (!IsLocalPlayer)
            return;
        cameraController.OnPlayerMove(camRotationAnchor, transform);


        if (Input.GetKeyDown(KeyCode.B))
        {
            TakeDamage(10);
        }

        if (Input.GetKeyDown(KeyCode.H))
        {
            UpdateHealthServerRpc(maxHealth, OwnerClientId);
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
            Vector2 direction = ctx.ReadValue<Vector2>();

            movement.x = direction.x;
            movement.z = direction.y;
        }
        else
        {
            var velocity = rigidBody.velocity;
            rigidBody.AddForce(-new Vector3(velocity.x, 0, velocity.z), ForceMode.VelocityChange);
            movement = Vector3.zero;
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
        transform.Rotate(Vector3.up, rotation.x * sensitivity);

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

        if (this.jetpack.IsFlying)
        {
            yDirection = ctx.ReadValueAsButton() ? 1 : 0;
        }

        if (ctx.interaction is MultiTapInteraction && ctx.performed)
        {
            this.yDirection = 0;
            this.jetpack.IsFlying = !this.jetpack.IsFlying;
        }
        else if (!this.jetpack.IsFlying)
        {
            Jump();
        }
    }

    private void Jump()
    {
        if (isGrounded)
            rigidBody.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
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
        this.jetpack.IsSwift = isRunning;
    }

    public void OnDash(InputAction.CallbackContext ctx)
    {
        if (!IsLocalPlayer || !cdManager.RequestDash())
            return;

        dashStartedSince = Time.fixedDeltaTime;

        dashDirection = Vector3.ClampMagnitude(rigidBody.velocity, 1f);

        if (dashDirection == Vector3.zero)
        {
            dashDirection = transform.TransformDirection(Vector3.forward);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
            isGrounded = true;
    }

    private void OnCollisionExit(Collision other)
    {
        if (other.gameObject.CompareTag("Ground"))
            isGrounded = false;
    }

    /**
         * <summary>
         *      Removes health from the player
         * </summary>
         * <param name="damage">int for amount of damage taken</param>
         */
    public bool TakeDamage(int damage)
    {
        if (damage >= currentHealth.Value)
        {
            UpdateHealthServerRpc(0, this.OwnerClientId);
            return false;
        }
        else
        {
            UpdateHealthServerRpc(currentHealth.Value - damage, this.OwnerClientId);
            return true;
        }
    }


    public void OnFire(InputAction.CallbackContext ctx)
    {
        isShooting = ctx.ReadValueAsButton();
    }

    /**
     * <summary>shoots at from the middle of the screen to where looked at</summary>
     * <returns>true if an damageable object has been shot</returns>
     */
    private bool Shoot()
    {
        GameObject hit;

        if ((hit = getObjectInSight()) == null)
            return false;

        if (hit.CompareTag("Player"))
        {
            PlayerController player = hit.GetComponent<PlayerController>();
            if (player == null)
            {
                return false;
            }


            int weaponDamage = 10;

            player.TakeDamage(weaponDamage);
        }
        else
        {
            // none of the tags has been found
            return false;
        }

        return true;
    }

    private void handleDash()
    {
        if (dashDirection == Vector3.zero)
            return;

        if (dashStartedSince > dashDuration)
        {
            dashStartedSince = 0;
            dashDirection = Vector3.zero;
            rigidBody.velocity = Vector3.zero;
        }
        else if (IsDashing)
        {
            dashStartedSince += Time.fixedDeltaTime;

            // a function : [0;1] => [0;1] with f(1) = 0
            // it acts as a smoothing function for the velocity change
            Func<float, float> kernelFunction = x => Mathf.Exp(-5 * (float) Math.Pow(2 * x - 1, 2));

            Vector3 velocity = dashDirection * (kernelFunction(dashStartedSince / dashDuration) * dashForce);
            rigidBody.AddForce(velocity - rigidBody.velocity, ForceMode.VelocityChange);
        }
    }

    /**
     * <summary>a method to get the <see cref="GameObject"/> in the line sight</summary>
     */
    private GameObject getObjectInSight()
    {
        Vector2 crosshairPosition = new Vector2(0.5f, 0.5f);

        Vector3 origin = cameraController.Camera.ViewportToWorldPoint(crosshairPosition);

        RaycastHit hit;

        // TODO : add weapon range accordingly to weapon
        float weaponRange = 30f;


        if (!Physics.Raycast(origin, cameraController.Camera.transform.forward, out hit, weaponRange))

            return null;

        return hit.collider.gameObject;
    }


    [ServerRpc(RequireOwnership = false)]
    public void UpdateHealthServerRpc(int newHealth, ulong playerId)
    {
        PlayerController damagedPlayer = NetworkManager.Singleton.ConnectedClients[playerId].PlayerObject
            .GetComponent<PlayerController>();

        damagedPlayer.currentHealth.Value = newHealth;
    }
}