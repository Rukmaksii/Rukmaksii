using System;
using Map;
using Minions;
using model;
using MonstersControler;
using PlayerControllers;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

namespace Weapons
{
    [RequireComponent(typeof(NetworkObject))]
    public abstract class BaseWeapon : NetworkBehaviour, IWeapon
    {
        [SerializeField] public Sprite sprite;
        [SerializeField] public Transform Model;
        public abstract WeaponType Type { get; }
        private NetworkVariable<NetworkBehaviourReference> playerReference = new NetworkVariable<NetworkBehaviourReference>();

        public BasePlayer Player
        {
            set => UpdatePlayerServerRpc(new NetworkBehaviourReference(value));
            get => playerReference.Value.TryGet<BasePlayer>(out BasePlayer res) ? res : null;
        }

        public abstract float Range { get; }

        public abstract int Damage { get; }

        public abstract string Name { get; }

        /**
     * <value>the time between each bullet row</value>
     */
        public abstract float Cooldown { get; }

        /**
     * <value>the remaining time of the <see cref="Cooldown"/></value>
     */
        protected float remainingCD = 0f;

        public abstract int MaxAmmo { get; }

        protected NetworkVariable<int> currentAmmo = new NetworkVariable<int>();
        public int CurrentAmmo => currentAmmo.Value;

        public abstract float ReloadTime { get; }

        protected float remainingReloadTime;

        /**
        * <value>the current percentage of the reload </value>
        */
        public float ReloadRate => remainingReloadTime / ReloadTime;

        /**
        * <value>the number of bullet fired each time the fire is called </value>
        */
        public abstract int BulletsInRow { get; }

        /**
        * <value>the time in seconds between each bullet in a bullet row (when <see cref="BulletsInRow"/> is bigger than 1)</value>
        */
        public abstract float BulletsInRowSpacing { get; }

        /**
        * <value>the number of remaining bullets to send in a bullet row</value>
        */
        protected int remainingBulletsInRow;

        /**
        * <value>the current remaining time between the previous bullet and the next bullet in the bullet row</value>
        */
        protected float betweenBulletsCurrentCD;

        protected bool isReloading = false;
        protected bool isShooting = false;

        public bool IsReloading => isReloading;

        /** <value>whether the hit marker should be displayed or not</value> */
        private bool hitMarkerDisplayed;

        /** <value>the time since the hit marker is displayed</value> */
        private float hitMarkerSince;

        /** <value>the duration the hit marker is displayed</value> */
        private float hitMarkerDuration = 0.2f;

        /** <summary>action for when a player hits a target and if it is a player</summary> */
        public static event Action<bool> targetHit;

        /**
         * <value>the <see cref="CameraController.offset">camera offset</see> to be set when aiming</value>
         */
        public virtual Vector3 AimingOffset { get; } = new Vector3(0.3f, 0.3f, -0.7f);

        /**
         * <value>the hud to set when aiming</value>
         */
        public virtual GameObject AimingHUD { get; } = null;

        private Transform weapon;


        void Start()
        {
            if (!IsServer)
                return;
            currentAmmo.Value = MaxAmmo;
            Transform[] transforms = Player.GetComponentsInChildren<Transform>();
            foreach (Transform transform in transforms)
            {
                if (transform.CompareTag("Weapon"))
                {
                    weapon = transform;
                    break;
                }
            }

            CombineMesh();
        }

        void CombineMesh()
        {
            MeshFilter[] meshFilters = Model.GetComponentsInChildren<MeshFilter>();
            CombineInstance[] combine = new CombineInstance[meshFilters.Length];

            int i = 0;
            while (i < meshFilters.Length)
            {
                combine[i].mesh = meshFilters[i].sharedMesh;
                combine[i].transform = meshFilters[i].transform.localToWorldMatrix;
                //meshFilters[i].gameObject.SetActive(false);

                i++;
            }
            transform.GetComponent<MeshFilter>().sharedMesh = new Mesh();
            transform.GetComponent<MeshFilter>().sharedMesh.CombineMeshes(combine);
            transform.gameObject.SetActive(true);
        }

        void UpdateServer(float deltaTime)
        {
            if (CurrentAmmo == 0 && !isReloading)
            {
                Reload();
            }

            if (!isReloading)
            {
                if (BulletsInRow > 1)
                {
                    handleMultiBulletFire(Time.fixedDeltaTime);
                }
                else
                {
                    handleSingleBulletFire(Time.fixedDeltaTime);
                }
            }
            else
            {
                if (remainingReloadTime <= 0)
                {
                    isReloading = false;
                    remainingReloadTime = 0;
                    currentAmmo.Value = MaxAmmo;
                }
                else
                {
                    remainingReloadTime -= deltaTime;
                }
            }
        }

        void UpdateClient(float deltaTime)
        {
            if (hitMarkerDisplayed)
            {
                hitMarkerSince += deltaTime;
                if (hitMarkerSince > hitMarkerDuration)
                {
                    hitMarkerDisplayed = false;
                    hitMarkerSince = 0;
                    targetHit?.Invoke(false);
                }
            }
        }

        // Update is called once per frame
        void FixedUpdate()
        {
            if (IsServer)
            {
                if (weapon != null)
                {
                    this.transform.rotation = weapon.transform.rotation;
                    this.transform.position = weapon.transform.position;
                }
                UpdateServer(Time.fixedDeltaTime);
            }
            if (IsClient)
                UpdateClient(Time.fixedDeltaTime);
            
            this.transform.position = Player.transform.position;
            
        }

        void handleMultiBulletFire(float deltaTime)
        {
            if (remainingCD > 0)
            {
                remainingCD -= deltaTime;
                isShooting = false;
                return;
            }

            if (betweenBulletsCurrentCD > 0)
            {
                betweenBulletsCurrentCD -= deltaTime;
            }
            else if (isShooting)
            {
                if (remainingBulletsInRow > 0)
                {
                    remainingBulletsInRow--;
                    Shoot();
                    betweenBulletsCurrentCD = BulletsInRowSpacing;
                }
                else
                {
                    remainingCD = Cooldown;
                    remainingBulletsInRow = BulletsInRow;
                    betweenBulletsCurrentCD = BulletsInRowSpacing;
                    isShooting = false;
                }
            }
        }

        public void SwitchRender(bool enable)
        {
            this.GetComponent<MeshRenderer>().enabled = enable;
        }


        /**
     * <summary>handles the cooldown process between fire when <see cref="BulletsInRow"/> is 1</summary>
     */
        void handleSingleBulletFire(float deltaTime)
        {
            if (remainingCD > 0)
            {
                remainingCD -= deltaTime;
            }
            else if (isShooting && Player.IsShooting)
            {
                Shoot();
                remainingCD = Cooldown;
            }

            isShooting = false;
        }

        public void Fire()
        {
            isShooting = true;
        }

        public void Reload()
        {
            UpdateAmmoServerRpc(0);
            this.isReloading = true;
            remainingReloadTime = ReloadTime;
        }

        [ServerRpc(RequireOwnership = false)]
        private void UpdateAmmoServerRpc(int ammo)
        {
            currentAmmo.Value = ammo;
        }

        /**
         * <summary>shoots at from the middle of the screen to where looked at</summary>
         * <remarks>shoots 1 bullet</remarks>
         * <returns>true if an damageable object has been shot</returns>
         */
        private bool Shoot()
        {
            currentAmmo.Value--;
            GameObject hit;

            if ((hit = this.Player.GetObjectInSight(this.Range)) == null)
                return false;

            if (hit.CompareTag("Player"))
            {
                BasePlayer enemyPlayer = hit.GetComponent<BasePlayer>();
                if (enemyPlayer == null || !Player.CanDamage(enemyPlayer))
                {
                    return false;
                }

                enemyPlayer.TakeDamage(this.Damage);
            }
            else if (hit.CompareTag("Minion"))
            {
                BaseMinion minion = hit.GetComponent<BaseMinion>();
                if (minion == null || Player.TeamId == minion.TeamId)
                    return false;

                minion.TakeDamage(this.Damage);
            }
            else if (hit.CompareTag("Monster"))
            {
                MonsterController monster = hit.GetComponent<MonsterController>();
                monster.TakeDamage(this.Damage);
            }
            else if (hit.CompareTag("Base"))
            {
                BaseController baseObject = hit.GetComponent<BaseController>();
                if (baseObject == null || Player.TeamId == baseObject.TeamId)
                    return false;
                
                baseObject.TakeDamage(this.Damage);
            }
            else
            {
                // none of the tags has been found
                return false;
            }

            ClientRpcParams p = new ClientRpcParams
            {
                Send = new ClientRpcSendParams
                {
                    TargetClientIds = new ulong[] {Player.OwnerClientId}
                }
            };
            DisplayHitMarkClientRpc(p);

            return true;
        }

        [ClientRpc]
        private void DisplayHitMarkClientRpc(ClientRpcParams clientRpcParams = default)
        {
            hitMarkerDisplayed = true;
            targetHit?.Invoke(true);
        }

        public void Destroy()
        {
            RemoveServerRpc();
            Destroy(this);
        }

        [ServerRpc]
        private void RemoveServerRpc()
        {
            GetComponent<NetworkObject>().Despawn();
        }

        [ServerRpc]
        private void UpdatePlayerServerRpc(NetworkBehaviourReference playerRef)
        {
            this.playerReference.Value = playerRef;
        }
    }
}