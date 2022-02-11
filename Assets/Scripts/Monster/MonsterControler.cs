using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

public class MonsterControler : NetworkBehaviour
{
    private int life = 50;

    

    public int Life => life;

    
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(wait());
    }

    IEnumerator wait()
    {
        yield return new WaitForSeconds(5);
        MonsterAI monsterAI = gameObject.AddComponent(typeof(MonsterAI)) as MonsterAI;
        monsterAI.agent = gameObject.GetComponent<NavMeshAgent>();
    }
    
    // Update is called once per frame
    void Update()
    {
        
    }

    public void TakeDamage(int damage)
    {
        if (damage >= life)
        {
            UpdateHealthServerRpc(0, this.OwnerClientId);
        }
        else
        {
            UpdateHealthServerRpc(life - damage, this.OwnerClientId);
        }
    }
    [ServerRpc(RequireOwnership = false)]
    public void UpdateHealthServerRpc(int newHealth, ulong playerId)
    {
        MonsterControler damagedPlayer = NetworkManager.Singleton.ConnectedClients[playerId].PlayerObject
            .GetComponent<MonsterControler>();

        damagedPlayer.life = newHealth;
    }
}
