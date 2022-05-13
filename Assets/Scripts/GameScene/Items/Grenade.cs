using GameScene.model;
using GameScene.PlayerControllers.BasePlayer;
using Unity.Netcode;
using UnityEngine;

namespace GameScene.Items
{
    public class Grenade : BaseItem
    {
        public override float Duration { get; protected set; } = 3f;
        private readonly int Damage = 50;
        private readonly float ThrowForce = 30f;
        public ParticleSystem explosion;
        public override int Price { get; set; } = 50;

        protected override void Setup()
        {
            NetworkObject.ChangeOwnership(NetworkManager.ServerClientId);
            transform.SetParent(null);
            transform.position = Player.transform.position;


            var rb = GetComponent<Rigidbody>();
            rb.isKinematic = false;
            rb.velocity = Vector3.zero;
            rb.AddForce(Player.AimVector * ThrowForce, ForceMode.Impulse);
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
        private void SpawnExplosionServerRpc()
        {
            ParticleSystem explo = Instantiate(explosion, transform.position, transform.rotation);
            var netObj = explo.GetComponent<NetworkObject>();
            netObj.Spawn();
        }
    }
}