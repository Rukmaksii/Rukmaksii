using UnityEngine;

public class CooldownManager : MonoBehaviour
{
    [SerializeField] protected float dashCooldown = 3f;

    private bool dashTriggered;
    private float dashedSince;

    public float DashCooldown => dashCooldown;
    public float DashedSince => dashedSince;


    [SerializeField] protected float minionCooldown = 10f;

    private float spawnedMinionSince = -1f;

    // Update is called once per frame
    void FixedUpdate()
    {
        if (dashTriggered)
        {
            dashedSince += Time.fixedTime;
            if (dashedSince > dashCooldown)
            {
                dashTriggered = false;
                dashedSince = 0;
            }
        }

        if (spawnedMinionSince >= 0f)
        {
            spawnedMinionSince += Time.fixedDeltaTime;
            if (spawnedMinionSince > minionCooldown)
                spawnedMinionSince = -1f;
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

    public bool RequestSpawnMinion()
    {
        if (spawnedMinionSince >= 0f)
            return false;
        spawnedMinionSince = 0f;
        return true;
    }
}