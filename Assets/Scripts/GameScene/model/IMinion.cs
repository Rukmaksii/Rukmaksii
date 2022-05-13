using GameScene.PlayerControllers.BasePlayer;
using UnityEngine;

namespace GameScene.model
{
    public interface IMinion : IKillable
    {
        /**
         * <summary>the strategy used for the minion</summary>
         */
        public enum Strategy
        {
            /**
             * <value>defend a spot on the map</value>
             */
            DEFEND = 0,

            /**
             * <value>Protects a player</value>
             */
            PROTECT = DEFEND + 1,

            /**
             * <value>attack enemy players and follow them</value>
             */
            ATTACK = PROTECT + 1,

            /**
             * <value>the number elements in the enum</value>
             */
            Count = ATTACK + 1
        }

        /**
         * <summary>aims at a target</summary>
         */
        public void Aim(Transform target);

        /**
         * <summary>moves to a given position</summary>
         */
        public void MoveTo(Vector3 position);

        /**
         * <summary>fires at a target</summary>
         */
        public void Fire(BasePlayer enemy);
    }
}