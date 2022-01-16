using UnityEngine;

namespace PlayerControllers
{
    public class Jetpack : MonoBehaviour
    {
        /**
    * <value>the multiplier for the fuel reload</value>
    */
        [SerializeField] private float reloadMultiplier = 2.5f;

        public PlayerController Player { get; set; }

        /**
         * <value>the max duration of the flight</value>
         */
        public float FuelDuration;

        /**
         * <value>the remaining time of use.</value>
         */
        private float currentFuelUse;

        public bool IsFlying
        {
            get => !this.Player.RigidBody.useGravity;
            set
            {
                if (value == true)
                {
                    if (this.CanTakeOff)
                    {
                        this.Player.RigidBody.useGravity = false;
                    }
                }
                else
                {
                    this.Player.RigidBody.useGravity = true;
                }
            }
        }


        /**
         * <value>the direction in world space to go to (magnitude should be under 1)</value>
         */
        private Vector3 _direction = Vector3.zero;

        public Vector3 Direction
        {
            get => _direction;
            set { _direction = Vector3.ClampMagnitude(value, 1f); }
        }

        /**
         * <value>the force that will be applied to the Player by the jetpack</value>
         */
        [SerializeField] private float jetpackForce = 5f;

        /**
         * <value>the minimum amount of fuel required to start the engines</value>
         */
        [SerializeField] private float minRequiredFuel = 0.25f;

        /**
     * <value>the max height the player can reach flying</value>
     */
        [SerializeField] private float maxHeight = 70f;

        /**
     * <value>the speed multiplier when the <see cref="Jetpack.IsSwift"/> flag is set</value>
     */
        [SerializeField] private float enhancedSpeedMultiplier = 2f;

        /**
     * <value>a flag allowing the player to go quicker (consuming more fuel) </value>
     */
        public bool IsSwift { get; set; }

        /**
     * <value>a float between 0 and 1 the percentage of fuel used</value>
     */
        public float FuelConsumption => currentFuelUse / FuelDuration;

        private bool isReady = false;

        public bool CanTakeOff => FuelConsumption >= minRequiredFuel;


        public void FixedUpdate()
        {
            HandleFuel();

            if (!isReady)
            {
                isReady = CanTakeOff;
            }

            if (IsFlying && isReady)
            {
                var velocity = Direction * jetpackForce;
                if (this.Player.transform.position.y >= maxHeight)
                {
                    velocity.y = 0;
                }

                if (IsSwift)
                {
                    velocity *= enhancedSpeedMultiplier;
                }

                // TODO : smooth out the velocity changes
                this.Player.RigidBody.velocity = velocity;
            }
        }


        private void HandleFuel()
        {
            float reloadTime = reloadMultiplier * Time.fixedDeltaTime;
            if (!this.IsFlying)
            {
                if (currentFuelUse + reloadTime > FuelDuration)
                {
                    currentFuelUse = FuelDuration;
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