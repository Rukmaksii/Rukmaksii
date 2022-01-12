using UnityEngine;

namespace DefaultNamespace
{
    public class Jetpack
    {
        
        public static readonly float RELOAD_MULTIPLIER = 2.5f;

        private PlayerController player;
        
        /**
         * <value>the max duration of the flight</value>
         */
        private readonly float fuelDuration;
        
        /**
         * <value>the remaining time of use.</value>
         */
        private float currentFuelUse;
        
        /**
         * <value>the force that will be applied to the player by the jetpack</value>
         */
        private float jetpackForce = 5f;

        public float FuelConsumption => currentFuelUse / fuelDuration;

        public Jetpack(PlayerController player, float fuelDuration)
        {
            this.player = player;
            this.fuelDuration = fuelDuration;
            currentFuelUse = this.fuelDuration;
        }

        public void Update(float time)
        {
            float reloadTime = RELOAD_MULTIPLIER * time;
            // should reload the 
            if (this.player.RigidBody.useGravity)
            {
                if (currentFuelUse + reloadTime > fuelDuration)
                {
                    currentFuelUse = fuelDuration;
                }
                else
                {
                    currentFuelUse += reloadTime;
                }
            }
            else
            {
                if (currentFuelUse - time < 0)
                {
                    currentFuelUse = 0;
                }
                else
                {
                    currentFuelUse -= time;
                }
            }
            
        }

        public void Fly(Vector3 direction)
        {
            this.player.RigidBody.AddForce(direction * jetpackForce, ForceMode.Impulse);
        }
    }
}