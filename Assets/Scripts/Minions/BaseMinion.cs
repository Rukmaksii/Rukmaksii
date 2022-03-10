using System;
using System.Collections.Generic;
using System.Linq;
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
    [RequireComponent(typeof(Collider))]
    public class BaseMinion : NetworkBehaviour, IMinion
    {
        [SerializeField] private float lookRadius = 10f;

        public virtual int MaxHealth { get; } = 50;

        private NetworkVariable<int> teamId = new NetworkVariable<int>(-1);
        private NetworkVariable<int> health = new NetworkVariable<int>(0);

        public int TeamId
        {
            get => teamId.Value;
            private set => UpdateTeamServerRpc(value);
        }

        public bool IsGrounded
        {
            get
            {
                RaycastHit hit;
                Vector3 initPos = transform.position;
                if (Physics.SphereCast(initPos, agent.height / 2, Vector3.down, out hit, 1))
                {
                    return hit.distance <= 0.2f && hit.collider.CompareTag("Ground");
                }


                return false;
            }
        }

        protected List<BasePlayer> Enemies =>
            GameController.Singleton.Players.FindAll(p =>
                p.TeamId != TeamId && Vector3.Distance(p.transform.position, transform.position) <= lookRadius);

        protected BasePlayer ClosestEnemy
        {
            get
            {
                var res = Enemies.OrderBy(p => Vector3.Distance(p.transform.position, transform.position)).ToList();
                return res.Count > 0 ? res.First() : null;
            }
        }

        protected List<BasePlayer> Allies =>
            GameController.Singleton.Players.FindAll(p =>
                p.TeamId == TeamId && Vector3.Distance(p.transform.position, transform.position) <= lookRadius);

        protected BasePlayer ClosestAlly
        {
            get
            {
                var res = Allies.OrderBy(p => Vector3.Distance(p.transform.position, transform.position)).ToList();
                return res.Count > 0 ? res.First() : null;
            }
        }

        protected BasePlayer owner;

        protected BasePlayer assignedPlayer;
        public IMinion.Strategy Strategy { get; set; }
        public Vector3 AssignedPosition { get; set; }

        protected Transform target;
        protected NavMeshAgent agent;

        void Start()
        {
            agent = GetComponent<NavMeshAgent>();
            UpdateHealthServerRpc(MaxHealth);
        }

        void Update()
        {
            // minion is not ready
            if (owner == null || !IsGrounded)
                return;

            var closest = ClosestEnemy;
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

        /**
         * <summary>Follows the <see cref="assignedPlayer"/></summary>
         */
        private void FollowPlayer()
        {
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

        /**
         * <summary>adds <see cref="delta"/> to the current life</summary>
         * <param name="delta">the delta to add</param>
         */
        [ServerRpc(RequireOwnership = false)]
        protected void UpdateHealthServerRpc(int delta)
        {
            this.health.Value += delta;
        }

        public bool TakeDamage(int damage)
        {
            if (health.Value <= damage)
            {
                OnKill();
                return false;
            }
            else
            {
                UpdateHealthServerRpc(-damage);
                return true;
            }
        }

        public void OnKill()
        {
            this.GetComponent<NetworkObject>().Despawn();
        }

        [ServerRpc(RequireOwnership = false)]
        private void DestroyServerRpc()
        {
            GetComponent<NetworkObject>().Despawn();
        }
    }
}