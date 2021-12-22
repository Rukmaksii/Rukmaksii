using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    
    // Constants to be set by unity
    [SerializeField] private float movementSpeed = 5F;
    [SerializeField] private float sensitivity = .1F;


    private Vector3 movement = Vector3.zero;
    
    private Rigidbody rigidBody;
    
    
    void Start()
    {
        rigidBody = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (movement != Vector3.zero)
        {
            Vector3 moveVector = Vector3.ClampMagnitude(movement, 1f);
            moveVector = transform.TransformVector(moveVector);
            rigidBody.MovePosition(transform.position + moveVector * Time.deltaTime * movementSpeed);
        }
        
    }

    public void OnMove(InputValue value)
    {
        Vector2 direction = value.Get<Vector2>();

        movement.x = direction.x;
        movement.z = direction.y;
    }

    public void OnRotation(InputValue value)
    {
        Vector2 rotation = value.Get<Vector2>();
        transform.Rotate(Vector3.down, rotation.x * sensitivity);
    }
}
