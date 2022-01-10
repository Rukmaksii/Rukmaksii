using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CooldownManager : MonoBehaviour
{
    [SerializeField] protected float dashCooldown = 3f;

    private bool dashTriggered = false;
    private float dashedSince = 0f;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (dashTriggered)
        {
            dashedSince += Time.deltaTime;
            if (dashedSince > dashCooldown)
            {
                dashTriggered = false;
                dashedSince = 0;
            }
        }
    }

    public bool RequestDash()
    {
        if (dashTriggered)
            return false;

        dashTriggered = true;

        return true;
    }
}