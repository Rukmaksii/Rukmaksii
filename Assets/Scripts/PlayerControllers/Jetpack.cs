using System;
using UnityEngine;

namespace PlayerControllers
{
    public class Jetpack : MonoBehaviour
    {
        /**
    * <value>the multiplier for the fuel reload</value>
    */
        [SerializeField] private float reloadMultiplier = 2.5f;

        public BasePlayer Player { get; set; }

        /**
         * <value>the max duration of the flight</value>
         */
        private float fuelDuration;

        public float FuelDuration
        {
            get => fuelDuration;
            set => fuelDuration = value;
        }

        /**
         * <value>the remaining time of use.</value>
         */
        private float currentFuelUse;

        public bool IsFlying
        {
            get => this.Player.IsFlying;
            set => Player.IsFlying = value && CanTakeOff;
        }


        /**
         * <value>the direction in world space to go to (magnitude should be under 1)</value>
         */
        private Vector3 _direction = Vector3.zero;

        public Vector3 Direction
        {
            get => _direction;
            set => _direction = Vector3.ClampMagnitude(value, 1f);
        }

        /**
         * <value>the force that will be applied to the Player by the jetpack</value>
         */
        [SerializeField] private float jetpackForce = 20f;

        /**
         * <value>the minimum amount of fuel required to start the engines</value>
         */
        [SerializeField] private float minRequiredFuel = 0.25f;

        /**
     * <value>the max height the player can reach flying</value>
     */
        [SerializeField] private float maxHeight = 20f;

        /**
     * <value>the speed multiplier when the <see cref="Jetpack.IsSwift"/> flag is set</value>
     */
        [SerializeField] private float enhancedSpeedMultiplier = 2f;

        /**
     * <value>a flag allowing the player to go quicker (consuming more fuel) </value>
     */
        public bool IsSwift => Player.IsRunning;

        /**
     * <value>a float between 0 and 1 the percentage of fuel used</value>
     */
        public float FuelConsumption => currentFuelUse / fuelDuration;

        private bool isReady = false;

        public bool CanTakeOff => FuelConsumption >= minRequiredFuel;


        public Vector3 Velocity
        {
            get
            {
                Vector3 direction = Vector3.ClampMagnitude(Player.Movement, 1f);
                float currentY = Player.transform.position.y;
                if (currentY > maxHeight)
                    direction.y = -1;
                // smoothing
                else if (Math.Abs(maxHeight - currentY) < 0.1 && direction.y > 0)
                    direction.y = 0;

                float multiplier = jetpackForce;
                if (IsSwift)
                    multiplier *= enhancedSpeedMultiplier;

                return direction * multiplier;
            }
        }


        public void FixedUpdate()
        {
            HandleFuel();

            if (!isReady)
            {
                isReady = CanTakeOff;
            }
        }


        private void HandleFuel()
        {
            float reloadTime = reloadMultiplier * Time.fixedDeltaTime;
            if (!this.IsFlying)
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
                float fuelConsumption = Time.fixedDeltaTime * (IsSwift ? enhancedSpeedMultiplier : 1f);
                if (currentFuelUse - fuelConsumption < 0)
                {
                    currentFuelUse = 0;
                    this.IsFlying = false;
                    this.isReady = false;
                }
                else
                {
                    currentFuelUse -= fuelConsumption;
                }
            }
        }
    }
}