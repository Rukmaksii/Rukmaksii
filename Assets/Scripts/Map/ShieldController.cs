using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ShieldController : NetworkBehaviour
{
    private NetworkVariable<bool> activated = new NetworkVariable<bool>(true);

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

    private new MeshCollider collider;
    
    // Start is called before the first frame update
    void Start()
    {
        collider = gameObject.GetComponent<MeshCollider>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ChangeActivation(bool activated)
    {
        collider.enabled = activated;
        this.activated.Value = activated;
    }
    
    [ServerRpc]
    public void UpdateTeamServerRpc(int teamId)
    {
        this.teamId = teamId;
    }
}

