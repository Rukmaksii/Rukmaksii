using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

public class CameraController : MonoBehaviour
{
    [SerializeField] private Vector3 offset = new Vector3(0f, 0, -2f);
    [SerializeField] private float baseAngle = -5F;
    [SerializeField] private float maxAngle = 90F;

    private float _addedAngle;

    public float AddedAngle
    {
        get => _addedAngle;
        set
        {
            if (Mathf.Abs(value) <= maxAngle)
            {
                _addedAngle = value;
            }
            else if (value < 0)
            {
                _addedAngle = -maxAngle;
            }
            else
            {
                _addedAngle = maxAngle;
            }
        }
    }


    private Camera cam;


    void Start()
    {
        cam = GetComponent<Camera>();
        _addedAngle = baseAngle;
    }

    public void OnPlayerMove(Vector3 camAnchor, Transform playerTransform)
    {

        var camTransform = cam.transform;
        
        Vector3 computedOffset = Quaternion.AngleAxis(AddedAngle, Vector3.right) * offset;
        Vector3 camPosition = camAnchor + playerTransform.TransformVector(computedOffset);
        camTransform.position = camPosition;

        Vector3 playerAngles = playerTransform.localEulerAngles;

        Vector3 camAngles = playerAngles + Vector3.right * AddedAngle;

        camTransform.localEulerAngles = camAngles;
    }

    // Update is called once per frame
    void Update()
    {
    }
}