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
    public class BaseMinion : NetworkBehaviour, IMinion
    {
        
        [SerializeField]
        private float lookRadius = 10f;

        private NetworkVariable<int> teamId = new NetworkVariable<int>(-1);

        public int TeamId
        {
            get => teamId.Value;
            private set => UpdateTeamServerRpc(value);
        }

        protected BasePlayer owner;

        public void BindOwner(BasePlayer owner)
        {
            this.owner = owner;
            TeamId = owner.TeamId;
            GameController.Singleton.AddMinion(this);
        }

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
            Gizmos.DrawWireSphere(transform.position, lookRadius);
        }

        [ServerRpc]
        private void UpdateTeamServerRpc(int teamId)
        {
            this.teamId.Value = teamId;
        }
    }
}