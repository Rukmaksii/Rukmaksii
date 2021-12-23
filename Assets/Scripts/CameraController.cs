using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private Vector3 offset = new Vector3(0f, 1f, -2f);
    [SerializeField] private float baseAngle = -5F;
    [SerializeField] private float maxAngle = 30F;

    private float _addedAngle = 0f;

    public float AddedAngle
    {
        get => _addedAngle;
        set
        {
            if (Mathf.Abs(_addedAngle) <= maxAngle)
            {
                _addedAngle = value;
            } else if (_addedAngle < 0)
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
    }

    public void OnPlayerMove(Vector3 camAnchor, Transform playerTransform)
    {
        Vector3 camPosition = camAnchor + playerTransform.TransformVector(offset);
        cam.transform.position = camPosition;

        Vector3 playerAngles = playerTransform.localEulerAngles;

        Vector3 camAngles = playerAngles + Vector3.right * (AddedAngle + baseAngle);

        cam.transform.localEulerAngles = camAngles;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
