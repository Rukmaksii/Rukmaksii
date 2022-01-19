using System;
using System.Collections;
using System.Collections.Generic;
using PlayerControllers;
using model;
using UnityEngine;


namespace Items
{
    public abstract class BaseItem : MonoBehaviour, IItems
    {
        public abstract ItemType Type { get; }
        
        public BasePlayer Player { get; set; }

        public abstract string Name { get; }

        public abstract void InitializePassive();

        public abstract void RemovePassive();

        public abstract  void Consume();

        public abstract void Use();
    }
}