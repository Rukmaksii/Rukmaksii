using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ShieldController : NetworkBehaviour
{
    private NetworkVariable<bool> activated = new NetworkVariable<bool>();

    public NetworkVariable<bool> Activated
    {
        get => activated;
        set => activated = value;
    }

    private int teamId;

    public int TeamId
    {
        get => teamId;
        set => teamId = value;
    }

    
    
    // Start is called before the first frame update
    void Start()
    {
        activated.Value = true;
    }

    // Update is called once per frame
    void Update()
    {
        MeshCollider collider = gameObject.GetComponent<MeshCollider>();
        collider.enabled = activated.Value;
    }

    public void ChangeActivation(bool activated)
    {
        this.activated.Value = activated;
    }
    
    [ServerRpc]
    public void UpdateTeamServerRpc(int teamId)
    {
        this.teamId = teamId;
    }
}