using System.Collections;
using System.Collections.Generic;
using PlayerControllers;
using UnityEngine;
using Weapons;

public class HandTarget : MonoBehaviour
{

    private BasePlayer player;

    private Transform[] weapons;

    private Transform[] refHand;

    // Start is called before the first frame update
    void Start()
    {
        player = GetComponentInParent<BasePlayer>();
    }

    // Update is called once per frame
    void Update()
    {
        if (this.gameObject.name == "right_target")
        {
            if (!player.IsLocalPlayer)
                return;
            weapons = player.GetComponentsInChildren<Transform>();
            foreach (Transform weapon in weapons)
            {
                if (weapon.CompareTag("Weapon"))
                {
                    if (weapon.name == player.Inventory.CurrentWeapon.Name)
                    {
                        refHand = weapon.GetComponentsInChildren<Transform>();
                        foreach (Transform tran in refHand)
                        {
                            if (tran.name == "ref_right_hand_target")
                            {
                                this.transform.position = tran.position;
                                this.transform.rotation = tran.rotation;
                            }
                        }
                    }
                }
            }
        }
        else if (this.gameObject.name == "left_target")
        {
            if (!player.IsLocalPlayer)
                return;
            weapons = player.GetComponentsInChildren<Transform>();
            foreach (Transform weapon in weapons)
            {
                if (weapon.CompareTag("Weapon"))
                {
                    if (weapon.name == player.Inventory.CurrentWeapon.Name)
                    {
                        refHand = weapon.GetComponentsInChildren<Transform>();
                        foreach (Transform tran in refHand)
                        {
                            if (tran.name == "ref_left_hand_target")
                            {
                                this.transform.position = tran.position;
                                this.transform.rotation = tran.rotation;
                            }
                        }
                    }
                }
            }
        }
    }
}