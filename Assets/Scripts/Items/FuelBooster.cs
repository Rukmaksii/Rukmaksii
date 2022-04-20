﻿using System;
using model;
using UnityEngine;

namespace Items
{
    public class FuelBooster : BaseItem
    {
        public override ItemCategory Type { get; } = ItemCategory.Other;
        public override string Name { get; } = "FuelBooster";

        // storing the old fuel value
        private float oldFuelValue = 10f;

        private float newFuelValue;

        protected override void Setup()
        {
            oldFuelValue = Player.Jetpack.FuelDuration;
            newFuelValue = 1f;
            Player.Jetpack.FuelDuration = newFuelValue;
        }
        
        protected override void OnConsume()
        {
        }

        protected override void TearDown()
        {
            Player.Jetpack.FuelDuration = oldFuelValue;
        }
    }
}