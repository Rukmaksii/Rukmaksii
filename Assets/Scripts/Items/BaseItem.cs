using System;
using System.Collections;
using System.Collections.Generic;
using PlayerControllers;
using model;
using UnityEngine;


namespace Items
{
    public abstract class BaseItem : MonoBehaviour, IItem
    {
        public abstract ItemType Type { get; }
        
        public BasePlayer Player { get; set; }

        public abstract string Name { get; }

        void Start()
        {
        }

        private void FixedUpdate()
        {
        }
    }
}