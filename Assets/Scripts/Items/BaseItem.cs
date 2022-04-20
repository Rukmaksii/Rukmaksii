using System;
using System.Collections.Generic;
using model;
using PlayerControllers;
using UnityEngine;

namespace Items
{
    public abstract class BaseItem : MonoBehaviour, IItem
    {
        public static Dictionary<Type, int> MaxDictionary = new Dictionary<Type, int> {
            {typeof(FuelBooster), 3}
        };
        public abstract ItemCategory Type { get; }
        public ItemState State { get; private set; } = ItemState.Clean;
        public abstract string Name { get; }
        public BasePlayer Player { get; set; }

        public void Consume()
        {
            throw new NotImplementedException();
        }

        protected abstract void Setup();

        protected abstract void OnConsume();
        protected abstract void TearDown();
    }
}