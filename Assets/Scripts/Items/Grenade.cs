using ExitGames.Client.Photon.StructWrapping;
using model;
using PlayerControllers;
using Unity.Netcode;
using UnityEngine;

namespace Items
{
    public class Grenade : BaseItem
    {
        public override float Duration { get; protected set; } = 3f;
        private int Damage = 50;
        private float ThrowForce = 30f;
        public ParticleSystem explosion;
        
        protected override void Setup()
        {
            GetComponent<Rigidbody>().isKinematic = false;
            transform.SetParent(null);
            Rigidbody rb = gameObject.GetComponent<Rigidbody>();
            Collider[] c = Player.gameObject.GetComponents<Collider>();
            gameObject.GetComponent<Collider>().enabled = false;
            rb.AddForce(Player.AimVector * ThrowForce, ForceMode.Impulse);
        }

        protected override void OnConsume()
        {
            gameObject.GetComponent<Collider>().enabled = true;
        }

        protected override void TearDown()
        {
            SpawnExplosionServerRPC();
            
            Collider[] colliders = Physics.OverlapSphere(transform.position,5f);

            foreach (Collider hit in colliders)
            {
                IKillable component = hit.GetComponent<IKillable>();

                if (component != null)
                {
                    if (!(component is BasePlayer && hit is CapsuleCollider))
                        component.TakeDamage(Damage);
                }
            }
        }

        [ServerRpc]
        private void SpawnExplosionServerRPC()
        {
            GameObject explo = Instantiate(explosion, transform.position, transform.rotation).gameObject;
            explo.GetComponent<NetworkObject>().Spawn();
        }
    }
}