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

        void Start()
        {
            throw new NotImplementedException();
        }

        private void FixedUpdate()
        {
            throw new NotImplementedException();
        }
    }
}