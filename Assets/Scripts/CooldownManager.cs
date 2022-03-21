using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class CooldownManager : NetworkBehaviour
{
    [SerializeField] protected float dashCooldown = 3f;

    public float DashCooldown => dashCooldown;

    private NetworkVariable<bool> canDash = new NetworkVariable<bool>(true);

    public float DashedSince { get; private set; } = 0f;

    public bool CanDash => canDash.Value;

    public bool CanSpawnMinion => canSpawnMinion.Value;

    public float SpawnedMinionSince { get; private set; } = 0f;
    [SerializeField] protected float minionCooldown = 10f;

    private NetworkVariable<bool> canSpawnMinion = new NetworkVariable<bool>(true);

    // Update is called once per frame
    public bool RequestDash()
    {
        if (!CanDash)
            return false;
        HandleDashCDServerRpc();
        return true;
    }

    private void FixedUpdate()
    {
        if (!CanDash)
            DashedSince += Time.fixedDeltaTime;
        else
            DashedSince = 0f;

        if (!CanSpawnMinion)
            SpawnedMinionSince += Time.fixedDeltaTime;
        else
            SpawnedMinionSince = 0f;
    }

    public bool RequestSpawnMinion()
    {
        if (!CanSpawnMinion)
            return false;
        HandleSpawnCDServerRpc();
        return true;
    }

    IEnumerator HandleDashCD()
    {
        if (IsServer)
        {
            yield return new WaitForSeconds(dashCooldown);
            canDash.Value = true;
        }
    }

    [ServerRpc]
    private void HandleDashCDServerRpc()
    {
        canDash.Value = false;
        StartCoroutine(HandleDashCD());
    }

    IEnumerator HandleSpawnCD()
    {
        if (IsServer)
        {
            yield return new WaitForSeconds(minionCooldown);
            canSpawnMinion.Value = true;
        }
    }

    [ServerRpc]
    private void HandleSpawnCDServerRpc()
    {
        canSpawnMinion.Value = false;
        StartCoroutine(HandleSpawnCD());
    }
}