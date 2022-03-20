using model;
using PlayerControllers;
using UnityEngine;

namespace Items
{
    public abstract class BaseItem : MonoBehaviour, IItem
    {
        public abstract ItemType Type { get; }
        public abstract string Name { get; }

        public BasePlayer Player { get; set; }

        public abstract void Start();
        public abstract void Update();
        public abstract void OnDestroy();
    }
}