using Unity.Netcode;
using UnityEngine;

namespace GameScene.Map
{
    [RequireComponent(typeof(MeshCollider))]
    public class ShieldController : NetworkBehaviour
    {
        private NetworkVariable<bool> activated = new NetworkVariable<bool>();

        public NetworkVariable<bool> Activated
        {
            get => activated;
            set => activated = value;
        }

        private int teamId;

        public int TeamId
        {
            get => teamId;
            set => teamId = value;
        }

        private new MeshCollider collider;
        private MeshRenderer meshRenderer;
    
        // Start is called before the first frame update
        void Start()
        {
            activated.Value = true;
            collider = gameObject.GetComponent<MeshCollider>();
            meshRenderer = gameObject.GetComponent<MeshRenderer>();
        }

        // Update is called once per frame
        void Update()
        {
            collider.enabled = meshRenderer.enabled = activated.Value;
        }

        [ServerRpc]
        public void UpdateTeamServerRpc(int teamId)
        {
            this.teamId = teamId;
        }
    }
}