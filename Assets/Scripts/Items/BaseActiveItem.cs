using System;
using System.Collections;
using System.Collections.Generic;
using PlayerControllers;
using model;
using UnityEngine;


namespace Items
{
    public abstract class BaseActiveItem : MonoBehaviour, IIActiveItem
    {
        public abstract ItemType Type { get; }
        
        public BasePlayer Player { get; set; }

        public abstract string Name { get; }
        public abstract void Start();
        public abstract void Update();
        public abstract void OnDestroy();
        public abstract void Use();
    }
}