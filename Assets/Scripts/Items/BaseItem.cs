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
        
        public BasePlayer Player { get; set; }


        public abstract string Name { get; }

        private bool _started = false;

        private void Start()
        {
            this._started = Player != null;
        }

        public void Update()
        {
            throw new NotImplementedException();
        }

        public void OnDestroy()
        {
            throw new NotImplementedException();
        }

        private void FixedUpdate()
        {
            if (!this._started)
            {
                Start();
                return;
            }
            
            this.OnPassiveCalled();
        }

        public abstract void OnStartPassive();
        public abstract void OnPassiveCalled();
        public abstract void OnRemovePassive();
        void IItem.Start()
        {
            Start();
        }
    }
}