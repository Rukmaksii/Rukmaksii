using System;
using Unity.Netcode;
using Unity.Netcode.Samples;
using UnityEngine;
using UnityEngine.InputSystem;
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
public class PlayerController : NetworkBehaviour
{
    // Constants to be set by unity
    [SerializeField] private float movementSpeed = 10F;

    /**
     * <value>the speed multiplier when running</value>
     */
    [SerializeField] private float runningSpeedMultiplier = 2f;

    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private float sensitivity = .1F;


    private bool isRunning = false;

    public bool IsRunning => isRunning;

    protected CameraController cameraController;

    protected GameController gameController;

    // the world space point the camera will rotate around
    protected Vector3 camRotationAnchor
    {
        get
        {
            var collider = GetComponent<CapsuleCollider>();
            return transform.TransformPoint(collider.center + Vector3.up * (collider.height / 4));
        }
    }

    private Vector3 movement = Vector3.zero;

    protected Rigidbody rigidBody;

    protected bool isGrounded = false;


    private Vector3 dashDirection;

    /**
     * <value>the duration of the dash in seconds</value>
     */
    [SerializeField] protected float dashDuration = 0.2F;
    [SerializeField] protected float dashForce = 40f;
    
    private float dashStartedSince = -1f;


    public bool IsDashing => dashStartedSince > 0 && dashStartedSince <= dashDuration;

    void Start()
    {
        GameObject playerCamera = GameObject.FindGameObjectWithTag("Player Camera");
        cameraController = playerCamera.GetComponent<CameraController>();
        cameraController.OnPlayerMove(camRotationAnchor, transform);

        GameObject gameManager = GameObject.FindGameObjectWithTag("GameController");
        gameController = gameManager.GetComponent<GameController>();
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

            var speed = movementSpeed * (isRunning ? runningSpeedMultiplier : 1F);


            Vector3 deltaVelocity = new Vector3(rigidBody.velocity.x, 0f, rigidBody.velocity.z);

            rigidBody.AddForce(moveVector * speed - deltaVelocity, ForceMode.VelocityChange);
        }

        // forces the capsule to stand up
        playerTransform.eulerAngles = new Vector3(0, playerTransform.eulerAngles.y, 0);

    }

    // Update is called once per frame
    private void Update()
    {
        if (!IsLocalPlayer)
            return;
        cameraController.OnPlayerMove(camRotationAnchor, transform);
    }

    /**
     * <summary>
     *      Called when the move event is triggered within unity
     * </summary>
     * <param name="value">the <see cref="InputValue"/> giving the move axis values </param>
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
     * <param name="value">the <see cref="InputValue"/> giving the rotation delta </param>
     */
    public void OnRotation(InputAction.CallbackContext ctx)
    {
        if (!IsLocalPlayer || cameraController == null)
            return;
        Vector2 rotation = ctx.ReadValue<Vector2>();
        transform.Rotate(Vector3.up, rotation.x * sensitivity);

        cameraController.AddedAngle -= rotation.y * sensitivity;
    }

    public void OnJump()
    {
        if (!IsLocalPlayer)
            return;

        if (isGrounded)
            rigidBody.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }

    public void OnRun(InputAction.CallbackContext ctx)
    {
        if (!IsLocalPlayer)
            return;

        isRunning = ctx.performed;
    }

    public void OnDash(InputAction.CallbackContext ctx)
    {
        if (!IsLocalPlayer || dashStartedSince > 0)
            return;

        dashStartedSince = Time.deltaTime;

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
            dashStartedSince += Time.deltaTime;
            rigidBody.AddForce(dashDirection * dashForce - rigidBody.velocity, ForceMode.VelocityChange);
        }
    }
}