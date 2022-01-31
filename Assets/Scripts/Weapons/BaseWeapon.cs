using System;
using System.Collections;
using System.Collections.Generic;
using PlayerControllers;
using model;
using UnityEngine;

namespace Weapons
{
    public abstract class BaseWeapon : MonoBehaviour, IWeapon
    {
        public abstract WeaponType Type { get; }
        public BasePlayer Player { get; set; }

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

        protected int currentAmmo;
        public int CurrentAmmo => currentAmmo;

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
        public static event Action<bool> playerShot;

        void Start()
        {
            currentAmmo = MaxAmmo;
        }

        // Update is called once per frame
        void FixedUpdate()
        {
            if (currentAmmo == 0 && !isReloading)
            {
                Reload();
            }

            if (!isReloading)
            {
                if (BulletsInRow > 1)
                {
                    handleMultiBulletFire();
                }
                else
                {
                    handleSingleBulletFire();
                }
            }
            else
            {
                if (remainingReloadTime <= 0)
                {
                    isReloading = false;
                    remainingReloadTime = 0;
                    currentAmmo = MaxAmmo;
                }
                else
                {
                    remainingReloadTime -= Time.fixedDeltaTime;
                }
            }

            if (hitMarkerDisplayed)
            {
                hitMarkerSince += Time.deltaTime;
                if (hitMarkerSince > hitMarkerDuration)
                {
                    hitMarkerDisplayed = false;
                    hitMarkerSince = 0;
                    playerShot?.Invoke(false);
                }
            }
        }

        void handleMultiBulletFire()
        {
            if (remainingCD > 0)
            {
                remainingCD -= Time.fixedDeltaTime;
                isShooting = false;
                return;
            }

            if (betweenBulletsCurrentCD > 0)
            {
                betweenBulletsCurrentCD -= Time.fixedTime;
            } else if (isShooting)
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


        /**
     * <summary>handles the cooldown process between fire when <see cref="BulletsInRow"/> is 1</summary>
     */
        void handleSingleBulletFire()
        {
            if (remainingCD > 0)
            {
                remainingCD -= Time.fixedDeltaTime;
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
            this.isReloading = true;
            remainingReloadTime = ReloadTime;
        }

        /**
         * <summary>shoots at from the middle of the screen to where looked at</summary>
         * <remarks>shoots 1 bullet</remarks>
         * <returns>true if an damageable object has been shot</returns>
         */
        private bool Shoot()
        {
            currentAmmo--;
            GameObject hit;

            if ((hit = this.Player.GetObjectInSight(this.Range)) == null)
                return false;

            if (hit.CompareTag("Player"))
            {
                BasePlayer enemyPlayer = hit.GetComponent<BasePlayer>();
                if (enemyPlayer == null)
                {
                    return false;
                }

                enemyPlayer.TakeDamage(this.Damage);

                hitMarkerDisplayed = true;
                playerShot?.Invoke(true);
            }
            else
            {
                // none of the tags has been found
                return false;
            }

            return true;
        }
    }
}