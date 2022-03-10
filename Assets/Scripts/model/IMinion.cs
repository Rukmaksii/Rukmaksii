using UnityEngine;

namespace model
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
            DEFEND,
            /**
             * <value>Protects a player</value>
             */
            PROTECT,
            
            /**
             * <value>attack enemy players and follow them</value>
             */
            ATTACK
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
        public void Fire();
    }
}