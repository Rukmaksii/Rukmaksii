using GameScene.model;
using Unity.Netcode;
using UnityEngine;

namespace GameScene.Map
{
    public class BaseController : MonoBehaviour, IKillable
    {
        private int _currentHealth = 5000;

        public int CurrentHealth => _currentHealth;

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
            if (_currentHealth - damage < 0)
            {
                _currentHealth = 0;
                OnKill();
                return false;
            }

            _currentHealth -= damage;
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