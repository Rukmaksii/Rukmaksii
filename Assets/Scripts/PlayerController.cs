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
    [SerializeField] private float movementSpeed = 5F;
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

        if (movement != Vector3.zero)
        {
            Vector3 moveVector = Vector3.ClampMagnitude(movement, 1f);
            moveVector = transform.TransformVector(moveVector);
            rigidBody.MovePosition(playerTransform.position + moveVector * Time.deltaTime * movementSpeed);
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
    public void OnMove(InputValue value)
    {
        if (!IsLocalPlayer)
            return;
        Vector2 direction = value.Get<Vector2>();

        movement.x = direction.x;
        movement.z = direction.y;
    }

    
    /**
     * <summary>
     *      Called when the rotation event is triggered within unity
     * </summary>
     * <param name="value">the <see cref="InputValue"/> giving the rotation delta </param>
     */
    public void OnRotation(InputValue value)
    {
        if (!IsLocalPlayer)
            return;
        Vector2 rotation = value.Get<Vector2>();
        transform.Rotate(Vector3.up, rotation.x * sensitivity);

        cameraController.AddedAngle -= rotation.y * sensitivity;
    }

    public void OnJump()
    {
        if (!IsLocalPlayer)
            return;
        
        if(isGrounded)
            rigidBody.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
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
}