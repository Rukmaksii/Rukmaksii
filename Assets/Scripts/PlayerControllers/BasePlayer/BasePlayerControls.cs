﻿using System;
using model;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Interactions;
using Weapons;

namespace PlayerControllers
{
    public abstract partial class BasePlayer
    {
        /**
                 * <summary>
                 *      Called when the move event is triggered within unity
                 * </summary>
                 * <param name="ctx">the <see cref="InputAction.CallbackContext"/> giving the move axis values </param>
                 */
        public void OnMove(InputAction.CallbackContext ctx)
        {
            if (!IsOwner)
                return;

            if (ctx.performed)
            {
                Vector2 direction = ctx.ReadValue<Vector2>();
                Vector3 moveVector = new Vector3(direction.x, Movement.y, direction.y);

                UpdateMovement(moveVector);
            }
            else
            {
                var moveVector = new Vector3(0, Movement.y, 0);
                UpdateMovement(moveVector);
            }
        }


        /**
                 * <summary>
                 *      Called when the rotation event is triggered within unity
                 * </summary>
                 * <param name="ctx">the <see cref="InputAction.CallbackContext"/> giving the rotation delta </param>
                 */
        public void OnRotation(InputAction.CallbackContext ctx)
        {
            if (!IsOwner || cameraController == null)
                return;
            Vector2 rotation = ctx.ReadValue<Vector2>();
            UpdateRotationRpc(Vector3.up, rotation.x * sensitivity);

            cameraController.AddedAngle -= rotation.y * sensitivity;
        }

        /**
                 * <summary>
                 *      Called when the jump event is triggered within unity
                 * </summary>
                 */
        public void OnJump(InputAction.CallbackContext ctx)
        {
            if (!IsOwner)
                return;


            var moveVector = Movement;

            if (ctx.interaction is MultiTapInteraction && ctx.performed)
            {
                moveVector.y = 0;
                yVelocity = 0;
                this.Jetpack.IsFlying = !this.Jetpack.IsFlying;
            }
            else
            {
                moveVector.y = (ctx.started || ctx.performed) && !ctx.canceled ? 1 : 0;
            }

            UpdateMovement(moveVector);
        }

        public void OnLowerJetpack(InputAction.CallbackContext ctx)
        {
            if (!IsOwner)
                return;

            var currentMovement = Movement;
            if (ctx.performed)
            {
                currentMovement.y = -1;
            }
            else if (ctx.canceled)
            {
                currentMovement.y = 0;
            }

            // ServerRpc methods operate not changes if values are unchanged 
            // if(currentMovement != Movement)
            UpdateMovement(currentMovement);
        }

        public void OnReload(InputAction.CallbackContext _)
        {
            if (!IsOwner)
                return;
            BaseWeapon weapon = inventory.CurrentWeapon;
            if (weapon.CurrentAmmo < weapon.MaxAmmo)
            {
                inventory.CurrentWeapon.Reload();
                SetAim(false);
            }
        }

        /**
                 * <summary>called when run button is toggled</summary>
                 */
        public void OnRun(InputAction.CallbackContext ctx)
        {
            if (!IsOwner)
                return;


            UpdateFlagsServerRpc(PlayerFlags.RUNNING, !IsAiming && ctx.performed);
        }

        public void OnDash(InputAction.CallbackContext ctx)
        {
            if (!IsOwner || !cdManager.RequestDash())
                return;

            IsDashing = true;
            dashStartedSince = 0;
            Vector3 moveVector = Movement;
            if (moveVector == Vector3.zero)
                moveVector = Vector3.forward;
            _dashDirection = transform.TransformDirection(moveVector) * dashForce;
            SetAim(false);
        }

        public void OnFire(InputAction.CallbackContext ctx)
        {
            if (!IsOwner)
                return;
            IsShooting = ctx.ReadValueAsButton();
        }

        public void OnWeaponSwitch(InputAction.CallbackContext ctx)
        {
            if (!IsOwner || IsAiming)
                return;

            // mouse wheel control
            if (ctx.action.type == InputActionType.PassThrough)
            {
                float value = ctx.ReadValue<float>();
                if (value > 0)
                    inventory.PreviousWeapon();
                else if (value < 0)
                    inventory.NextWeapon();
                //change currentWeaponModel

                weapons = GetComponentsInChildren<Transform>();
                foreach (Transform weaponModel in weapons)
                {
                    if (weaponModel.CompareTag("Weapon"))
                    {
                        weaponModel.GetComponent<MeshRenderer>().enabled =
                            String.Equals(weaponModel.name, Inventory.CurrentWeapon.Name);
                        weaponRends = weaponModel.GetComponentsInChildren<Transform>();
                        foreach (Transform tran in weaponRends)
                        {
                            if (tran.GetComponent<MeshRenderer>() != null)
                            {
                                tran.GetComponent<MeshRenderer>().enabled =
                                    String.Equals(weaponModel.name, Inventory.CurrentWeapon.Name);
                            }
                        }
                    }
                }
            }
            else // 1,2,3 control
            {
                // TODO : implement 1,2,3 weapon switch control
            }
        }

        public void OnAim(InputAction.CallbackContext ctx)
        {
            if (!IsOwner)
                return;

            SetAim(!inventory.CurrentWeapon.IsReloading && ctx.performed);
        }

        public void OnChangeStrategy(InputAction.CallbackContext ctx)
        {
            if (!IsOwner || !ctx.started)
                return;
            strategy = (IMinion.Strategy) ((1 + (int) strategy) % (int) IMinion.Strategy.Count);
        }

        public void OnSpawnMinion(InputAction.CallbackContext ctx)
        {
            if (!IsOwner || !ctx.started)
                return;

            var tr = this.transform;
            SpawnMinionServerRpc(strategy, tr.position - tr.forward, tr.rotation);
        }
    }
}