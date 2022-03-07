using System.Collections;
using System.Collections.Generic;
using PlayerControllers;
using UnityEngine;
using Weapons;

public class HandTarget : MonoBehaviour
{

    private BasePlayer player;

    private GameObject[] weapons;

    private Transform[] refHand;

    // Start is called before the first frame update
    void Start()
    {
        player = GetComponentInParent<BasePlayer>();
        weapons = GameObject.FindGameObjectsWithTag("Weapon");
    }

    // Update is called once per frame
    void Update()
    {
        if (this.gameObject.name == "right_target")
        {
            foreach (GameObject weapon in weapons)
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
        else if (this.gameObject.name == "left_target")
        {
            foreach (GameObject weapon in weapons)
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