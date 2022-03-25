using System.Collections;
using model;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

namespace MonstersControler
{
    [RequireComponent(typeof(NetworkObject))]
    public class MonsterControler : NetworkBehaviour, IKillable
    {
        [SerializeField] private int maxHealth = 50;
        private NetworkVariable<int> life = new NetworkVariable<int>(0);

        public int Life => life.Value;
        private NavMeshAgent agent;

        // Start is called before the first frame update
        void Start()
        {
            UpdateLifeServerRpc(maxHealth);
            agent = gameObject.GetComponent<NavMeshAgent>();
            StartCoroutine(wait());
        }

        IEnumerator wait()
        {
            if (agent.isOnNavMesh)
            {
                MonsterAI monsterAI = gameObject.AddComponent(typeof(MonsterAI)) as MonsterAI;
                monsterAI.agent = gameObject.GetComponent<NavMeshAgent>();
            }
            else
            {
                yield return new WaitForSeconds(2);
                StartCoroutine(wait());
            }
        }
        

        // Update is called once per frame
        void Update()
        {
        }

        public bool TakeDamage(int damage)
        {
            if (damage >= life.Value)
            {
                OnKill();
                return false;
            }
            else
            {
                UpdateLifeServerRpc(-damage);
                return true;
            }
        }

        public void OnKill()
        {
            DestroyServerRpc();
        }

        /**
         * <summary>adds the <see cref="delta"/> to life</summary>
         * <param name="delta">the delta to add to the life</param>
         */
        [ServerRpc(RequireOwnership = false)]
        private void UpdateLifeServerRpc(int delta)
        {
            life.Value += delta;
        }

        [ServerRpc(RequireOwnership = false)]
        private void DestroyServerRpc()
        {
            GetComponent<NetworkObject>().Despawn();
        }
    }
}