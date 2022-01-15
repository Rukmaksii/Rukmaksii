using System.Collections;
using System.Collections.Generic;
using model;
using UnityEngine;

public abstract class BaseWeapon : MonoBehaviour, IWeapon
{

    [SerializeField] protected float range;

    public float Range => range;
    
    [SerializeField] protected float damage;
    public float Damage => damage;
    
    [SerializeField] protected float cooldown;
    public float Cooldown => cooldown;
    
    [SerializeField] protected int maxAmmo;
    public int MaxAmmo => maxAmmo;
    
    protected int currentAmmo;
    public int CurrentAmmo => currentAmmo;

    [SerializeField] protected float reloadTime;
    public float ReloadTime => reloadTime;
    
    protected float remainingReloadTime;

    /**
     * <value>the current percentage of the reload </value>
     */
    public float ReloadRate => remainingReloadTime / reloadTime;

    /**
     * <value>the number of bullet fired each time the fire is called </value>
     */
    [SerializeField] protected int sentBulletsInRow;

    /**
     * <value>the time in seconds between each bullet in a bullet row (when <see cref="sentBulletsInRow"/> is bigger than 1)</value>
     */
    [SerializeField] protected float bulletsInRowSpacing;

    /**
     * <value>the number of remaining bullets to send in a bullet row</value>
     */
    protected int remainingBulletsInRow;

    /**
     * <value>the current remaining time between the previous bullet and the next bullet in the bullet row</value>
     */
    protected float betweenBulletsCurrentCD;
    
    
    // Update is called once per frame
    void FixedUpdate()
    {
        
    }


    /**
     * <summary>handles the cooldown process between fire when <see cref="sentBulletsInRow"/> is 1</summary>
     */
    void handleSingleBulletFire()
    {
      // it is assumed that sentBulletsInRow is 1
      
    }

    public bool Fire()
    {
        throw new System.NotImplementedException();
    }

    public void Reload()
    {
        throw new System.NotImplementedException();
    }
}
