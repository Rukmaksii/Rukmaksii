using GameManagers;
using Minions;
using model;
using Unity.Netcode;
using UnityEngine;

namespace PlayerControllers
{
    public abstract partial class BasePlayer
    {
        /**
         * <summary>spawns a minion bound to the player</summary>
         */
        [ServerRpc]
        protected void SpawnMinionServerRpc(IMinion.Strategy strat, Vector3 position, Quaternion rotation)
        {
            if (Minions.Count >= maxMinions || !cdManager.RequestSpawnMinion())
                return;

            GameObject instance = Instantiate(GameController.Singleton.MinionPrefab, position, rotation);
            instance.GetComponent<NetworkObject>().Spawn();
            BaseMinion minion = instance.GetComponent<BaseMinion>();
            minion.BindOwnerServerRpc(this.OwnerClientId, strat);
        }


        [ServerRpc(RequireOwnership = false)]
        public void UpdateHealthServerRpc(int newHealth, ulong playerId)
        {
            BasePlayer damagedPlayer = NetworkManager.Singleton.ConnectedClients[playerId].PlayerObject
                .GetComponent<BasePlayer>();

            damagedPlayer.CurrentHealth.Value = newHealth;
        }

        //[ServerRpc]
        //public void UpdateMovementServerRpc(Vector3 movement)
        public void UpdateMovement(Vector3 movement)
        {
            UpdateFlagsServerRpc(PlayerFlags.MOVING, movement.magnitude > 0);
            this.Movement = movement;
        }

        [ServerRpc]
        private void UpdateVelocityServerRpc(Vector3 velocity)
        {
            this.velocity.Value = velocity;
        }

        //[ServerRpc]
        public void UpdatePositionRpc(Vector3 position)
        {
            controller.Move(position);
        }

        //[ServerRpc]
        public void UpdateRotationRpc(Vector3 direction, float angle)
        {
            transform.Rotate(direction, angle);
        }

        // ownership ain't required since server has to be able to change the flags
        [ServerRpc(RequireOwnership = false)]
        private void UpdateFlagsServerRpc(PlayerFlags flag, bool add = true)
        {
            int value = flags.Value;

            if (add)
            {
                value |= (int) flag;
            }
            else
            {
                value &= (int) ~flag;
            }

            this.flags.Value = value;

            // flag specific settings
            switch (flag)
            {
                case PlayerFlags.DASHING:

                    break;
                case PlayerFlags.FLYING:
                    break;
            }
        }


        [ServerRpc]
        public void UpdateTeamServerRpc(int teamId)
        {
            this.teamId.Value = teamId;
        }

        [ServerRpc]
        public void UpdateAimVectorServerRpc(Vector3 castPoint, Vector3 direction)
        {
            aimVector = (castPoint, direction);
        }

        [ServerRpc]
        public void UpdateMoneyServerRpc(int money)
        {
            this.money = money;
        }

        [ServerRpc]
        private void OnKillServerRpc()
        {
            
        }
    }
}