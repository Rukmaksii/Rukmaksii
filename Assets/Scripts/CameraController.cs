using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private Vector3 offset = new Vector3(0f, 1f, -2f);
    [SerializeField] private float baseAngle = 5F;
    [SerializeField] private float maxAngle = 30F;


    private Camera cam;
    
    
    void Start()
    {
        cam = GetComponent<Camera>();
        // cam.transform.Rotate(Vector3.left, baseAngle);
    }

    public void OnPlayerMove(Vector3 camAnchor, Transform playerTransform, float addedAngle = 0f)
    {
        Vector3 camPosition = camAnchor + playerTransform.TransformVector(offset);
        cam.transform.position = camPosition;

        Vector3 playerAngles = playerTransform.eulerAngles;

        cam.transform.eulerAngles = playerAngles;

        if (addedAngle == 0F)
            return;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
