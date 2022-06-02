using System.Collections.Generic;
using GameScene.GameManagers;
using GameScene.Items;
using GameScene.Minions;
using GameScene.model;
using GameScene.model.Network;
using Unity.Netcode;
using UnityEngine;

namespace GameScene.PlayerControllers.BasePlayer
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
            GameController.Singleton.Scoreboard.UpdateData(this.OwnerClientId, PlayerInfoField.SpawnedMinions, 1, true);
        }


        [ServerRpc(RequireOwnership = false)]
        public void UpdateHealthServerRpc(int newHealth)
        {

            int delta = newHealth - CurrentHealth;
            if (delta >= 0)
            {
                GameController.Singleton.Scoreboard.UpdateData(OwnerClientId, PlayerInfoField.DamagesReceived, delta, true);
            }
            else
            {
                GameController.Singleton.Scoreboard.UpdateData(OwnerClientId, PlayerInfoField.HealingReceived, -delta, true);
            }

            currentHealth.Value = newHealth;
        }

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


        [ServerRpc(RequireOwnership = false)]
        public void UpdateTeamServerRpc(int teamId)
        {
            this.teamId.Value = teamId;
        }

        [ServerRpc]
        public void UpdateAimVectorServerRpc(Vector3 castPoint, Vector3 direction)
        {
            aimVector.Value = (castPoint, direction);
        }

        [ServerRpc(RequireOwnership = false)]
        public void UpdateMoneyServerRpc(int money)
        {
            this.money.Value = money;
        }

        [ServerRpc(RequireOwnership = false)]
        private void OnKillServerRpc()
        {
            GameController.Singleton.Scoreboard.UpdateData(OwnerClientId, PlayerInfoField.Deaths, 1, true);
            List<BaseItem> ListDropOne = new List<BaseItem>();
            foreach (var item in Inventory.ItemRegistry)
            {
                while (item.Value.TryPop(out BaseItem baseItem))
                {
                    foreach (var drop in ListDropOne)
                    {
                        Physics.IgnoreCollision(baseItem.gameObject.GetComponent<Collider>(),
                            drop.gameObject.GetComponent<Collider>());
                    }

                    baseItem.UnInteract();
                    baseItem.SwitchRender(true);
                    ListDropOne.Add(baseItem);
                }
            }

            foreach (BaseItem item in ListDropOne)
            {
                foreach (BaseItem baseItem in ListDropOne)
                {
                    Physics.IgnoreCollision(baseItem.gameObject.GetComponent<Collider>(),
                        item.gameObject.GetComponent<Collider>(), false);
                }
            }
        }
    }
}