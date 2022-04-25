using model;
using PlayerControllers;
using UnityEngine;

namespace Items
{
    public class Grenade : BaseItem
    {
        public override float Duration { get; protected set; } = 3f;
        private int Damage = 5;

        protected override void Setup()
        {
        }

        protected override void OnConsume()
        {
        }

        protected override void TearDown()
        {
            Debug.Log("Boom");
            
            Collider[] colliders = Physics.OverlapSphere(transform.position,5f);

            foreach (Collider hit in colliders)
            {
                IKillable component = hit.GetComponent<IKillable>();

                if (component != null)
                    component.TakeDamage(Damage);
            }
        }
    }
}