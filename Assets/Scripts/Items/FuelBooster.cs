using System;
using model;
using UnityEngine;

namespace Items
{
    public class FuelBooster : BaseItem
    {
        public override string Name { get; } = "FuelBooster";

        // storing the old fuel value
        private float oldFuelValue = 10f;

        private float newFuelValue;

        void Start()
        {
            oldFuelValue = Player.Jetpack.FuelDuration;
            newFuelValue = 1f;
            Player.Jetpack.FuelDuration = newFuelValue;
        }
        

        private float intialFuelDuration;

        public override void OnStartPassive()
        {
            intialFuelDuration = Player.Jetpack.FuelDuration;
            Player.Jetpack.FuelDuration = 1f;
        }

        public override void OnPassiveCalled()
        {
           // does nothing 
        }

        public override void OnRemovePassive()
        {
            Player.Jetpack.FuelDuration = this.intialFuelDuration;
        }

    }
}