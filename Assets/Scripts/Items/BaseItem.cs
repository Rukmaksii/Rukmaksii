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
        public abstract string Name { get; }
        
        public BasePlayer Player { get; set; }

        public abstract void Start();
        public abstract void Update();
        public abstract void OnDestroy();
        public abstract void OnStartPassive();
        public abstract void OnPassiveCalled();
        public abstract void OnRemovePassive();
    }
}