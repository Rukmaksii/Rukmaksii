using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

namespace MonstersControler
{
    
    [RequireComponent(typeof(NetworkObject))]
    public class MonsterControler : NetworkBehaviour
    {
        private NetworkVariable<int> life = new NetworkVariable<int>();

        public int Life => life.Value;


        // Start is called before the first frame update
        void Start()
        {
            life.Value = 50;
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

        public void TakeDamage(int damage, MonsterControler monster)
        {

            if (damage >= life.Value)
            {
                if (monster != null)
                {
                    Destroy(monster.gameObject);
                }
                else
                {
                    Debug.Log("monster is null");
                }
            }
            else
            {
                life.Value -= damage;
            }
        }
    }
}
