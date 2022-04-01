using System.Collections;
using System.Collections.Generic;
using model;
using Unity.Netcode;
using UnityEngine;

namespace Map
{
    public class BaseController : NetworkBehaviour, IKillable
    {
        private NetworkVariable<int> CurrentHealth { get; } = new NetworkVariable<int>(1);

        private NetworkVariable<int> teamId = new NetworkVariable<int>(-1);

        public int TeamId => teamId.Value;

        void Start()
        {
            
        }

        void Update()
        {
            
        }

        public bool TakeDamage(int damage)
        {
            if (CurrentHealth.Value - damage < 0)
            {
                CurrentHealth.Value = 0;
                OnKill();
                return false;
            }

            CurrentHealth.Value -= damage;
            return true;
        }

        public void OnKill()
        {
            Debug.Log("Base destroyed");
        }
    }
}