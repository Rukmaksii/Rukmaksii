using System.Linq;
using Items;
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

            if (ctx.interaction is MultiTapInteraction multiTapInteraction &&
                multiTapInteraction.tapCount > 1 &&
                ctx.time - ctx.startTime < 0.3)
            {
                if (ctx.performed)
                {
                    moveVector.y = 0;
                    yVelocity = 0;
                    Jetpack.IsFlying = !Jetpack.IsFlying;
                }
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
            SpawnMinionServerRpc(strategy, GroundPosition - tr.forward, tr.rotation);
        }

        public void OnDrop(InputAction.CallbackContext ctx)
        {
            if (!IsOwner || !ctx.performed)
                return;
            Inventory.DropCurrentWeapon();
        }

        public void OnPickUp(InputAction.CallbackContext ctx)
        {
            if (!IsOwner || focusedObject == null)
                return;

            if (focusedObject.TryGetComponent(out BaseWeapon weapon))
                inventory.AddWeapon(weapon);
        }

        private GameObject[] GetSurroundingObjects(float distance)
        {
            // ReSharper disable once Unity.PreferNonAllocApi
            return Physics.OverlapSphere(transform.TransformPoint(controller.center), distance)
                .Where(cld => cld.gameObject.TryGetComponent(out BaseWeapon weapon) && !weapon.IsOwned)
                .Select(cld => cld.gameObject)
                .ToArray();
        }

        private GameObject GetClosestPickableObject(float distance)
        {
            return GetSurroundingObjects(distance)
                .Where(go =>
                {
                    var position = cameraController.Camera.WorldToViewportPoint(go.transform.position);
                    var screenPos = new Vector2(position.x, position.y);
                    return screenPos.y > 0 && screenPos.y < 1 && screenPos.x > 0 && screenPos.x < 1;
                })
                .OrderBy(go => Vector3.Distance(transform.position, go.transform.position))
                .FirstOrDefault();
        }

        public void OnInventoryOpened(InputAction.CallbackContext ctx)
        {
            if (!IsOwner || !ctx.performed)
                return;

            var container = inventory.GetItemContainer<FuelBooster>();
            Debug.Log(container.Count);
            inventory.AddItem(container.Peek());
        }
    }
}