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
        public PlayerController Player { get; set; }

        public abstract float Range { get; }

        public abstract int Damage { get; }

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

        /**
     * <summary>a flag used for multi bullets in a row, true if the player triggered fire and released before the end of the bullet row</summary>
     */
        private bool hasBeenFiring = false;

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
                    // handleMultiBulletFire();
                }
                else
                {
                    handleSingleBulletFire();
                }
            }
            else
            {
                Debug.Log("reloading");
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
        }


        /**
     * <summary>handles the cooldown process between fire when <see cref="BulletsInRow"/> is 1</summary>
     */
        void handleSingleBulletFire()
        {
            if (remainingCD > 0)
            {
                remainingCD -= Time.fixedDeltaTime;
                return;
            }

            if (this.Player.IsShooting)
            {
                Shoot();
                remainingCD = Cooldown;
            }
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
            Debug.Log("shooting");
            currentAmmo--;
            GameObject hit;

            if ((hit = this.Player.GetObjectInSight(this.Range)) == null)
                return false;

            if (hit.CompareTag("Player"))
            {
                PlayerController enemyPlayer = hit.GetComponent<PlayerController>();
                if (enemyPlayer == null)
                {
                    return false;
                }


                enemyPlayer.TakeDamage(this.Damage);
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