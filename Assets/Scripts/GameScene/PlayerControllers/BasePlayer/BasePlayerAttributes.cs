using System;
using System.Collections.Generic;
using GameScene.Abilities.model;
using GameScene.GameManagers;
using GameScene.Minions;
using GameScene.model;
using GameScene.Shop;
using GameScene.Shop.ShopUI;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.Serialization;

namespace GameScene.PlayerControllers.BasePlayer
{
    public abstract partial class BasePlayer
    {
        public abstract string ClassName { get; }
        protected virtual float movementSpeed { get; } = 5f;

        /**
                * <value>the speed multiplier when running</value>
                */
        protected virtual float runningSpeedMultiplier { get; } = 2f;

        protected virtual float jumpForce { get; } = 5f;

        /**
                * <value>the mouse sensitivity</value>
                */
        [SerializeField] private float sensitivity = .1F;


        [SerializeField] private float pickUpDistance = 10F;

        private GameObject focusedObject = null;


        public bool IsRunning
        {
            get
                => this.HasFlag(PlayerFlags.MOVING) && this.HasFlag(PlayerFlags.RUNNING) &&
                   velocity.Value.z >= Mathf.Abs(velocity.Value.x);
            private set => UpdateFlagsServerRpc(PlayerFlags.RUNNING, value);
        }


        [SerializeField] private Transform weaponContainer;

        public Transform WeaponContainer => weaponContainer.transform;

        private CameraController cameraController;

        [SerializeField] private RigBuilder rigBuilder;

        public CameraController CameraController => cameraController;

        protected CooldownManager cdManager;

        // the world space point the camera will rotate around
        protected Vector3 camRotationAnchor =>
            transform.TransformPoint(controller.center + Vector3.up * (controller.height / 4));

        protected Vector3 FireCastPoint
        {
            get
            {
                var cld = GetComponent<CapsuleCollider>();
                Vector2 crosshairPosition = new Vector2(0.5f, 0.5f);

                Vector3 origin = cameraController.Camera.ViewportToWorldPoint(crosshairPosition);
                origin += cameraController.gameObject.transform.forward * cameraController.Offset.magnitude;
                return origin;
            }
        }

        protected virtual float gravityMultiplier { get; } = 1;

        /**
             * <value>stores the x,y,z inputs of the player</value>
             */
        // private readonly NetworkVariable<Vector3> movement = new NetworkVariable<Vector3>(Vector3.zero);

        public Vector3 Movement { get; private set; } = Vector3.zero;

        /**
                 * <value>the yVelocity of the player</value>
                 * <remarks>this velocity is only accessible to and used by the server</remarks>
                 */
        private float yVelocity = 0f;

        private NetworkVariable<Vector3> velocity = new NetworkVariable<Vector3>(Vector3.zero);

        /**
                 * <value>the local velocity of the player</value>
                 * <remarks>the velocity vector is only available on the server</remarks>
                 */
        public Vector3 Velocity => IsFlying ? Jetpack.Velocity : velocity.Value;

        /**
                 * <value>the projected position on the ground when not grounded</value>
                 */
        public Vector3 GroundPosition
        {
            get
            {
                Vector3 res = transform.position;
                RaycastHit hit;
                if (!IsGrounded &&
                    Physics.Raycast(transform.TransformPoint(controller.center - controller.height / 2 * Vector3.up),
                        Vector3.down, out hit) && hit.collider.CompareTag("Ground"))
                {
                    res.y -= hit.distance - controller.height / 2;
                }

                return res;
            }
        }


        private CharacterController controller;


        public bool IsGrounded
        {
            get
            {
                RaycastHit hit;
                var cld = GetComponent<CapsuleCollider>();
                Vector3 initPos = cld.transform.TransformPoint(cld.center);
                if (Physics.Raycast(initPos, Vector3.down, out hit, cld.height / 2))
                {
                    return hit.collider.CompareTag("Ground") || hit.collider.CompareTag("Base");
                }


                return false;
            }
        }

        public bool IsFlying
        {
            get => HasFlag(PlayerFlags.FLYING) && !IsGrounded;
            set => UpdateFlagsServerRpc(PlayerFlags.FLYING, value);
        }


        public bool IsShooting
        {
            get => HasFlag(PlayerFlags.SHOOTING);
            set => UpdateFlagsServerRpc(PlayerFlags.SHOOTING, value);
        }

        public bool IsAiming
        {
            get => HasFlag(PlayerFlags.AIMING);
            private set => UpdateFlagsServerRpc(PlayerFlags.AIMING, value);
        }


        public Inventory.Inventory Inventory => GetComponent<Inventory.Inventory>();
        public Jetpack Jetpack => GetComponent<Jetpack>();

        /** <value>the duration of the dash in seconds</value> */
        protected virtual float dashDuration { get;} = 0.3f;


        protected virtual float dashForce { get; } = 30f;

        public abstract int MaxHealth { get; protected set; }

        /** <value>current player health</value> */
        private readonly NetworkVariable<int> currentHealth = new NetworkVariable<int>(1);

        private NetworkVariable<int> teamId = new NetworkVariable<int>(-1);

        public int TeamId => teamId.Value;

        private NetworkVariable<int> flags = new NetworkVariable<int>(0);

        public int CurrentHealth
        {
            get => currentHealth.Value;
            set => UpdateHealthServerRpc(value);
        }

        /**
                * <value>the time in seconds since the dash has been called</value>
                */
        private float dashStartedSince = -1f;

        public bool IsDashing
        {
            get => HasFlag(PlayerFlags.DASHING);
            set => UpdateFlagsServerRpc(PlayerFlags.DASHING, value);
        }

        // getters for respectively the default dash cooldown and the time since last dash
        public float DashCooldown => cdManager.DashCooldown;
        public float DashedSince => cdManager.DashedSince;

        private Vector3 _dashDirection = Vector3.zero;

        // default value for fuel duration
        public virtual float DefaultFuelDuration { get; } = 10f;

        [SerializeField] private int maxMinions = 2;

        public List<BaseMinion> Minions => GameController.Singleton.Minions.FindAll(m => m.OwnerId == OwnerClientId);

        private IMinion.Strategy strategy = IMinion.Strategy.PROTECT;
        public IMinion.Strategy Strategy => strategy;

        /**
         * <summary>the fire cast point and the camera </summary>
         */
        private NetworkVariable<(Vector3, Vector3)> aimVector =
            new NetworkVariable<(Vector3, Vector3)>((Vector3.zero, Vector3.zero));

        public Vector3 AimVector => aimVector.Value.Item2;


        private bool itemWheel = false;
        public bool ItemWheel => itemWheel;
        private Vector3 mousePos = Vector3.zero;

        private NetworkVariable<int> money = new NetworkVariable<int>();

        public int Money
        {
            get => money.Value;
            set => UpdateMoneyServerRpc(value);
        }

        public abstract RootAbility RootAbility { get; }

        [SerializeField] private Sprite sprite;

        public Sprite Sprite => sprite;

        public abstract Type WeaponInterface { get; }

        private PlayerState playerState;


        private ShopController currentShop;
    }
}