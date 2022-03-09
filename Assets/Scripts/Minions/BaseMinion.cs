using System;
using model;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using UnityEngine.AI;

namespace Minions
{
    [RequireComponent(typeof(NetworkTransform))]
    [RequireComponent(typeof(NetworkRigidbody))]
    [RequireComponent(typeof(NavMeshAgent))]
    public class BaseMinion : NetworkBehaviour, IMinion
    {
        
        [SerializeField]
        private float lookRadius = 10f;
        
        public void Aim()
        {
            throw new System.NotImplementedException();
        }

        public void MoveTo(Vector3 position)
        {
            throw new System.NotImplementedException();
        }

        public void Fire()
        {
            throw new System.NotImplementedException();
        }


        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
        }
    }
}