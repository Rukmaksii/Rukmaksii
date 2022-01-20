using PlayerControllers;
using UnityEngine;

public class CooldownManager : MonoBehaviour
{
    [SerializeField] protected float dashCooldown = 3f;

    private bool dashTriggered;
    private float dashedSince;

    public float DashCooldown => dashCooldown;
    public float DashedSince => dashedSince;
    
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
    
    public float RequestDashCooldown()
    {
        return dashedSince;
    }
    
    public float RequestMaxDashCooldown()
    {
        return dashCooldown;
    }
    
    public bool RequestDash()
    {
        if (dashTriggered)
            return false;

        dashTriggered = true;

        return true;
    }
}