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

        public override void Start()
        {
            oldFuelValue = Player.Jetpack.FuelDuration;
            newFuelValue = 1f;
            Player.Jetpack.FuelDuration = newFuelValue;
        }
        
        public override void Update()
        {
        }

        public override void OnDestroy()
        {
            Player.Jetpack.FuelDuration = oldFuelValue;
        }

        public override void OnStartPassive()
        {
            throw new NotImplementedException();
        }

        public override void OnPassiveCalled()
        {
            throw new NotImplementedException();
        }

        public override void OnRemovePassive()
        {
            throw new NotImplementedException();
        }
    }
}