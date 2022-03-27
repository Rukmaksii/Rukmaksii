using System.Collections;
using System.Collections.Generic;
using PlayerControllers;
using UnityEngine;
using Weapons;

public class HandTarget : MonoBehaviour
{

    private BasePlayer player;

    private Transform weapon;

    private Transform leftHand;

    private Transform rightHand;

    // Start is called before the first frame update
    void Start()
    {
        player = GetComponentInParent<BasePlayer>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!player.IsLocalPlayer)
            return;

        weapon = player.Inventory.CurrentWeapon.Model;

        Transform[] transforms = weapon.GetComponentsInChildren<Transform>();

        foreach (Transform transform in transforms)
        {
            if (transform.CompareTag("LeftHand"))
                leftHand = transform;
            if (transform.CompareTag("RightHand"))
                rightHand = transform;
        }
        
        if (this.gameObject.name == "right_target")
        {
            this.transform.localPosition = rightHand.transform.position;
            this.transform.localRotation = rightHand.transform.rotation;

        }
        else if (this.gameObject.name == "left_target")
        {
            this.transform.localPosition = leftHand.transform.position;
            this.transform.localRotation = leftHand.transform.rotation;
        }
    }
}