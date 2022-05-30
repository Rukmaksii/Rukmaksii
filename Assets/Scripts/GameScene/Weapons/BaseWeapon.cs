using System;
using GameScene.GameManagers;
using GameScene.Map;
using GameScene.Minions;
using GameScene.model;
using GameScene.model.Network;
using GameScene.Monster;
using GameScene.PlayerControllers;
using GameScene.PlayerControllers.BasePlayer;
using Unity.Netcode;
using Unity.Netcode.Samples;
using UnityEngine;

namespace GameScene.Weapons
{
    [RequireComponent(typeof(NetworkObject))]
    [RequireComponent(typeof(ClientNetworkTransform))]
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(AudioSource))]
    public abstract class BaseWeapon : NetworkBehaviour, IWeapon, IPickable
    {
        [SerializeField] private Sprite sprite;

        public Sprite Sprite => sprite;
        [SerializeField] private Transform model;

        public abstract WeaponType Type { get; }

        private NetworkVariable<NetworkBehaviourReference> playerReference =
            new NetworkVariable<NetworkBehaviourReference>();


        [SerializeField] private Transform _rightHandTarget;
        [SerializeField] private Transform _leftHandTarget;

        public Transform RightHandTarget => _rightHandTarget;

        public Transform LeftHandTarget => _leftHandTarget;

        public BasePlayer Player
        {
            set =>
                UpdatePlayerServerRpc(value is null
                    ? new NetworkBehaviourReference()
                    : new NetworkBehaviourReference(value));

            get => IsSpawned && playerReference.Value.TryGet<BasePlayer>(out BasePlayer res) ? res : null;
        }

        /// <summary>
        ///     checks if the weapon is owned by a player
        /// </summary>
        public bool IsOwned => !(Player is null);

        private Transform Shoulder;

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

        private NetworkVariable<bool> renderState = new NetworkVariable<bool>(true);

        private AudioSource source;

        protected abstract int Price { get; }

        private void Awake()
        {
            source = GetComponent<AudioSource>();
            renderState.OnValueChanged += (_, val) => SwitchRenderers(val);
            playerReference.OnValueChanged += (_, val) => SwitchColliders(!IsOwned);
        }

        void Start()
        {
            if (IsServer)
                currentAmmo.Value = MaxAmmo;
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

            var rb = GetComponent<Rigidbody>();
            rb.isKinematic = IsOwned;

            if (IsServer && !IsOwned)
            {
                if (rb.velocity.magnitude > 10f)
                {
                    rb.velocity = rb.velocity.normalized * 10f;
                }
            }

            if (IsOwner && IsOwned)
            {
                if (Shoulder == null)
                    SetShoulder();
                transform.localPosition = Player.transform.InverseTransformPoint(Player.WeaponContainer.position);
                transform.localRotation = Shoulder.transform.localRotation * Player.WeaponContainer.localRotation;
            }
            else
            {
                Shoulder = null;
            }
        }

        // Update is called once per frame
        void FixedUpdate()
        {
            if (!IsSpawned)
                return;
            if (IsServer)
                UpdateServer(Time.fixedDeltaTime);


            if (IsClient)
                UpdateClient(Time.fixedDeltaTime);
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

        public void SwitchRender(bool render)
        {
            if (!IsServer)
                throw new NotServerException("only server can switch render");


            renderState.Value = render;
            if (IsClient)
                SwitchRenderers(render);
        }

        private void SwitchRenderers(bool render)
        {
            foreach (var renderer in GetComponentsInChildren<MeshRenderer>())
                renderer.enabled = render;
        }

        private void SwitchColliders(bool collide)
        {
            foreach (var collider in GetComponentsInChildren<Collider>())
                collider.enabled = collide;
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
         * <remarks>this method is only executed on server</remarks>
         * <remarks>shoots 1 bullet</remarks>
         * <returns>true if an damageable object has been shot</returns>
         */
        private bool Shoot()
        {
            if (!IsServer)
                throw new NotServerException();
            PlaySoundClientRPC();

            currentAmmo.Value--;
            GameObject hit;

            if ((hit = this.Player.GetObjectInSight(this.Range)) == null)
                return false;

            var scoreboard = GameController.Singleton.Scoreboard;
            if (hit.CompareTag("Player"))
            {
                BasePlayer enemyPlayer = hit.GetComponent<BasePlayer>();
                if (enemyPlayer == null || !Player.CanDamage(enemyPlayer))
                {
                    return false;
                }

                scoreboard.UpdateData(Player.OwnerClientId, PlayerInfoField.DamagesDone, this.Damage, true);
                if (this.Damage >= enemyPlayer.CurrentHealth)
                {
                    scoreboard.UpdateData(Player.OwnerClientId, PlayerInfoField.Kill, 1, true);
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
                if (this.Damage >= monster.Life)
                {
                    scoreboard.UpdateData(Player.OwnerClientId, PlayerInfoField.MonstersKilled, 1, true);
                }

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
                    TargetClientIds = new[] {Player.OwnerClientId}
                }
            };
            DisplayHitMarkClientRpc(p);

            return true;
        }

        [ClientRpc]
        private void PlaySoundClientRPC()
        {
            AudioSource source = gameObject.GetComponent<AudioSource>();
            AudioSource.PlayClipAtPoint(source.clip, gameObject.transform.position, source.volume);
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

        [ServerRpc(RequireOwnership = false)]
        private void UpdatePlayerServerRpc(NetworkBehaviourReference playerRef)
        {
            this.playerReference.Value = playerRef;
            if (playerRef.TryGet(out BasePlayer _))
                SwitchColliders(false);
            else
                SwitchColliders(true);
        }

        private void SetShoulder()
        {
            Transform[] transforms = Player.GetComponentsInChildren<Transform>();
            foreach (Transform transform in transforms)
            {
                if (transform.name == "mixamorig:RightShoulder")
                {
                    Shoulder = transform;
                    break;
                }
            }
        }

        /// <summary>
        ///     binds a new owner to the weapon 
        /// </summary>
        /// <param name="player">the new owner of the weapon</param>
        /// <exception cref="NotServerException">if not server</exception>
        public void PickUp(BasePlayer player)
        {
            if (!IsServer)
                throw new NotServerException();
            Player = player;
            NetworkObject.ChangeOwnership(player.OwnerClientId);
            NetworkObject.TrySetParent(player.transform);
            SetShoulder();
        }

        /// <summary>
        ///     unbinds the player from the weapon
        /// </summary>
        /// <exception cref="NotServerException">if not server</exception>
        public void Drop()
        {
            if (!IsServer)
                throw new NotServerException();

            NetworkObject.ChangeOwnership(NetworkManager.ServerClientId);
            transform.SetParent(null);
            transform.SetPositionAndRotation(Player.transform.position, Player.transform.rotation);
            Player = null;
        }
    }
}