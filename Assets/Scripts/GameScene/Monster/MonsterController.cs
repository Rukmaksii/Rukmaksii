using System.Collections.Generic;
using System.Net.Http.Headers;
using GameScene.GameManagers;
using GameScene.model;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

namespace GameScene.Monster
{
    [RequireComponent(typeof(NetworkObject))]
    [RequireComponent(typeof(MonsterAI))]
    public class MonsterController : NetworkBehaviour, IKillable
    {
        [SerializeField] private int maxHealth = 50;

        [SerializeField] private List<Material> materials = new List<Material>();
        private readonly NetworkVariable<int> life = new NetworkVariable<int>(0);

        public int Life
        {
            get => life.Value;
            set => UpdateLifeServerRpc(value);
        }

        public int MaxHealth => maxHealth;

        // Start is called before the first frame update
        void Start()
        {
            gameObject.GetComponentInChildren<SkinnedMeshRenderer>().material =
                materials[Random.Range(0, materials.Count)];
        }


        // Update is called once per frame
        void Update()
        {
        }

        public bool TakeDamage(int damage)
        {
            if (damage >= Life)
            {
                OnKill();
                return false;
            }
            else
            {
                Life -= damage;
                return true;
            }
        }

        public void OnKill()
        {
            Gameloop.Singleton.RemoveMonster(this);
        }

        /**
         * <summary>adds the <see cref="delta"/> to life</summary>
         * <param name="delta">the delta to add to the life</param>
         */
        [ServerRpc(RequireOwnership = false)]
        private void UpdateLifeServerRpc(int value)
        {
            life.Value = value;
        }

        [ServerRpc(RequireOwnership = false)]
        public void DestroyServerRpc()
        {
            GetComponent<NetworkObject>().Despawn();
        }
    }
}