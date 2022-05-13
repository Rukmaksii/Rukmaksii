using GameScene.model;
using Unity.Netcode;
using UnityEngine;

namespace GameScene.Map
{
    public class BaseController : MonoBehaviour, IKillable
    {
        private int currentHealth = 100;

        public int CurrentHealth => currentHealth;

        private int teamId = -1;

        public int TeamId => teamId;
        
        void Start()
        {
            
        }

        void Update()
        {
            
        }

        public bool TakeDamage(int damage)
        {
            if (currentHealth - damage < 0)
            {
                currentHealth = 0;
                OnKill();
                return false;
            }

            currentHealth -= damage;
            return true;
        }

        public void OnKill()
        {
            Debug.Log("Base destroyed");
        }
        
        [ServerRpc]
        public void UpdateTeamServerRpc(int teamId)
        {
            this.teamId = teamId;
        }
    }
}