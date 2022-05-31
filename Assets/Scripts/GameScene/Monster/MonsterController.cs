using GameScene.GameManagers;
using GameScene.model;
using Unity.Mathematics;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

namespace GameScene.Monster
{
    [RequireComponent(typeof(NetworkObject))]
    [RequireComponent(typeof(MonsterAI))]
    public class MonsterController : NetworkBehaviour, IKillable
    {
        [SerializeField] private int maxHealth = 50;
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
            Gameloop.Singleton.ListOfMonster.Add(this);
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
            Gameloop.Singleton.ListOfMonster.Remove(this);
            DestroyServerRpc();
            GameObject GrenadeInstance = Instantiate(GameController.Singleton.ItemPrefabs[1],
                gameObject.transform.position, quaternion.identity);
            GrenadeInstance.GetComponent<NetworkObject>().Spawn();
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
        private void DestroyServerRpc()
        {
            GetComponent<NetworkObject>().Despawn();
        }
    }
}