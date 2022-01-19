using UnityEngine;

public class CooldownManager : MonoBehaviour
{
    [SerializeField] protected float dashCooldown = 3f;

    private bool dashTriggered;
    private float dashedSince;


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
    
    public bool RequestDash()
    {
        if (dashTriggered)
            return false;

        dashTriggered = true;

        return true;
    }
}