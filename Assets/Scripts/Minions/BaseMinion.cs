using System;
using System.Collections.Generic;
using GameManagers;
using model;
using PlayerControllers;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using UnityEngine.AI;

namespace Minions
{
    [RequireComponent(typeof(NetworkTransform))]
    [RequireComponent(typeof(NetworkRigidbody))]
    [RequireComponent(typeof(NavMeshAgent))]
    [RequireComponent(typeof(NetworkObject))]
    public class BaseMinion : NetworkBehaviour, IMinion
    {
        [SerializeField] private float lookRadius = 10f;

        private NetworkVariable<int> teamId = new NetworkVariable<int>(-1);

        public int TeamId
        {
            get => teamId.Value;
            private set => UpdateTeamServerRpc(value);
        }

        protected BasePlayer owner;

        protected Transform target;
        protected NavMeshAgent agent;

        void Start()
        {
            agent = GetComponent<NavMeshAgent>();
        }

        void Update()
        {
            // minion is not ready
            if (owner == null)
                return;
            List<BasePlayer> enemies = GameController.Singleton.Players.FindAll(p => p.TeamId != TeamId);

            float minDistance = -1;
            BasePlayer closest = null;
            foreach (var enemy in enemies)
            {
                float distance = Vector3.Distance(transform.position, enemy.transform.position);
                if (distance <= lookRadius)
                {
                    if (closest == null || distance < minDistance)
                    {
                        minDistance = distance;
                        closest = enemy;
                    }
                }
            }

            if (closest != null)
            {
                MoveTo(closest.transform.position);
            }
        }

        public void BindOwner(BasePlayer owner)
        {
            this.owner = owner;
            TeamId = owner.TeamId;
        }

        public void Aim()
        {
            throw new NotImplementedException();
        }

        public void MoveTo(Vector3 position)
        {
            agent.SetDestination(position);
        }

        public void Fire()
        {
            throw new NotImplementedException();
        }


        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, lookRadius);
        }

        [ServerRpc]
        private void UpdateTeamServerRpc(int teamId)
        {
            this.teamId.Value = teamId;
        }
    }
}