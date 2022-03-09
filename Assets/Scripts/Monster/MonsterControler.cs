using System;
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


        // Start is called before the first frame update
        void Start()
        {
            UpdateLifeServerRpc(maxHealth);
            StartCoroutine(wait());
        }

        IEnumerator wait()
        {
            yield return new WaitForSeconds(15);
            MonsterAI monsterAI = gameObject.AddComponent(typeof(MonsterAI)) as MonsterAI;
            monsterAI.agent = gameObject.GetComponent<NavMeshAgent>();
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
            Destroy(this.gameObject);
        }

        /**
         * <summary>adds the <see cref="delta"/> to life</summary>
         * <param name="delta">the delta to add to the life</param>
         */
        [ServerRpc]
        private void UpdateLifeServerRpc(int delta)
        {
            life.Value += delta;
        }
    }
}
