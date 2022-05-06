using ExitGames.Client.Photon.StructWrapping;
using model;
using PlayerControllers;
using Unity.Netcode;
using Unity.Netcode.Components;
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
            UnparentServerRpc();
        }

        protected override void OnConsume()
        {
            gameObject.GetComponent<Collider>().enabled = true;
        }

        protected override void TearDown()
        {
            SpawnExplosionServerRpc();
            
            Collider[] colliders = Physics.OverlapSphere(transform.position, 5f);

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
        private void UnparentServerRpc()
        {
            NetworkObject.RemoveOwnership();
            transform.SetParent(null);
            GetComponent<Rigidbody>().isKinematic = false;
            Rigidbody rb = gameObject.GetComponent<Rigidbody>();
            Collider[] c = Player.gameObject.GetComponents<Collider>();
            gameObject.GetComponent<Collider>().enabled = false;
            rb.AddForce(Player.AimVector * ThrowForce, ForceMode.Impulse);
        }

        [ServerRpc]
        private void SpawnExplosionServerRpc()
        {
            ParticleSystem explo = Instantiate(explosion, transform.position, transform.rotation);
            var netObj = explo.GetComponent<NetworkObject>();
            netObj.Spawn();
        }
    }
}