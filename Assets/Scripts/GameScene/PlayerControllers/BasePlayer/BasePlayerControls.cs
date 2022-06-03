using System.Linq;
using GameScene.GameManagers;
using GameScene.HUD;
using GameScene.model;
using GameScene.Shop;
using GameScene.Weapons;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Interactions;

namespace GameScene.PlayerControllers.BasePlayer
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
            if (!IsOwner || playerState != PlayerState.Normal)
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
            if (!IsOwner || cameraController == null || itemWheel || playerState != PlayerState.Normal)
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
            if (!IsOwner || playerState != PlayerState.Normal)
                return;


            var moveVector = Movement;

            if (ctx.interaction is MultiTapInteraction multiTapInteraction &&
                multiTapInteraction.tapCount > 1)
            {
                if (ctx.performed &&
                    ctx.time - ctx.startTime < 0.3)
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
            if (!IsOwner || playerState != PlayerState.Normal)
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
            if (!IsOwner || playerState != PlayerState.Normal)
                return;
            BaseWeapon weapon = Inventory.CurrentWeapon;
            if (weapon.CurrentAmmo < weapon.MaxAmmo)
            {
                Inventory.CurrentWeapon.Reload();
                SetAim(false);
            }
        }

        /**
                 * <summary>called when run button is toggled</summary>
                 */
        public void OnRun(InputAction.CallbackContext ctx)
        {
            if (!IsOwner || playerState != PlayerState.Normal)
                return;


            UpdateFlagsServerRpc(PlayerFlags.RUNNING, !IsAiming && ctx.performed);
        }

        public void OnDash(InputAction.CallbackContext ctx)
        {
            if (!IsOwner || !cdManager.RequestDash() || playerState != PlayerState.Normal)
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
            if (!IsOwner || playerState != PlayerState.Normal)
                return;
            if (Inventory.SelectedMode == PlayerControllers.Inventory.Inventory.Mode.Item)
            {
                if (ctx.started)
                    Inventory.UseItem();
            }
            else if (Inventory.SelectedMode == PlayerControllers.Inventory.Inventory.Mode.Weapon)
            {
                if (ctx.started)
                    IsShooting = true;
                else if (ctx.canceled)
                    IsShooting = false;
            }
        }

        public void OnWeaponSwitch(InputAction.CallbackContext ctx)
        {
            if (!IsOwner || IsAiming || playerState != PlayerState.Normal)
                return;

            // mouse wheel control
            if (ctx.action.type == InputActionType.PassThrough)
            {
                float value = ctx.ReadValue<float>();
                if (value > 0)
                    Inventory.PreviousWeapon();
                else if (value < 0)
                    Inventory.NextWeapon();
            }
            else // 1,2,3 control
            {
                // TODO : implement 1,2,3 weapon switch control
            }
        }

        public void OnAim(InputAction.CallbackContext ctx)
        {
            if (!IsOwner || playerState != PlayerState.Normal && playerState != PlayerState.InMap
                                                              && playerState != PlayerState.InSelectStrat)
                return;

            if (playerState == PlayerState.InMap || playerState == PlayerState.InSelectStrat)
                OpenStratSelector(ctx);
            else
                SetAim(!Inventory.CurrentWeapon.IsReloading && ctx.performed);
        }

        private void OpenStratSelector(InputAction.CallbackContext ctx)
        {
            if (ctx.started)
            {
                playerState = PlayerState.InSelectStrat;
                GameController.Singleton.HUDController.ShowMinionSelection();
            }
            else if (ctx.canceled)
            {
                IMinion.Strategy strat = GameController.Singleton.HUDController.HideMinionSelection();
                if (strat != IMinion.Strategy.Count)
                {
                    strategy = strat;
                    var tr = transform;
                    SpawnMinionServerRpc(strategy, GroundPosition - tr.forward, tr.rotation);
                }
            }
        }

        public void OnChangeStrategy(InputAction.CallbackContext ctx)
        {
            if (!IsOwner || !ctx.started || playerState != PlayerState.Normal)
                return;
            strategy = (IMinion.Strategy) ((1 + (int) strategy) % (int) IMinion.Strategy.Count);
        }

        public void OnSpawnMinion(InputAction.CallbackContext ctx)
        {
            if (!IsOwner || playerState != PlayerState.Normal && playerState != PlayerState.InMap
                                                              && playerState != PlayerState.InSelectStrat)
                return;

            if (ctx.started)
            {
                GameController.Singleton.HUDController.ScaleUp(true);
                playerState = PlayerState.InMap;
                Cursor.lockState = CursorLockMode.Confined;
            }
            else if (ctx.canceled)
            {
                if (playerState == PlayerState.InSelectStrat)
                    GameController.Singleton.HUDController.HideMinionSelection();
                GameController.Singleton.HUDController.ScaleUp(false);
                playerState = PlayerState.Normal;
                Cursor.lockState = CursorLockMode.Locked;
            }
        }

        public void OnDrop(InputAction.CallbackContext ctx)
        {
            if (!IsOwner || !ctx.performed || playerState != PlayerState.Normal)
                return;
            Inventory.Drop();
        }

        public void OnInteract(InputAction.CallbackContext ctx)
        {
            if (!IsOwner || focusedObject == null || !ctx.performed)
                return;

            if (currentShop != null)
            {
                if (playerState == PlayerState.Normal)
                {
                    OpenShop(currentShop);
                }
                else
                {
                    CloseShop();
                }
            }
            else
            {
                Inventory.PickUpObject(focusedObject);
            }
        }

        private GameObject[] GetSurroundingObjects(float distance)
        {
            // ReSharper disable once Unity.PreferNonAllocApi
            return Physics.OverlapSphere(transform.TransformPoint(controller.center), distance)
                .Where(cld => cld.gameObject.TryGetComponent(out IInteractable pickable) && !pickable.IsInteractable)
                .Select(cld => cld.gameObject)
                .ToArray();
        }

        private GameObject GetClosestInteractableObject(float distance)
        {
            return GetSurroundingObjects(distance)
                .Where(go =>
                {
                    var transform1 = transform;
                    Vector3 direction = go.transform.position - transform1.position;
                    float product = Vector3.Dot(direction, transform1.forward);


                    return product > 0;
                })
                .OrderBy(go => Vector2.Distance(new Vector2(.5F, .5F), cameraController.Camera.WorldToViewportPoint(go.transform.position)))
                .FirstOrDefault();
        }

        private void OpenShop(ShopController controller)
        {
            if (controller.Interact(this))
            {
                playerState = PlayerState.InShop;
                Cursor.lockState = CursorLockMode.Confined;
            }
        }

        private void CloseShop()
        {
            currentShop.UnInteract();
            Cursor.lockState = CursorLockMode.Locked;
            playerState = PlayerState.Normal;
            currentShop = null;
        }


        public void OnInventoryOpened(InputAction.CallbackContext ctx)
        {
            if (!IsOwner || playerState != PlayerState.Normal)
                return;

            if (ctx.started)
            {
                itemWheel = true;
                Cursor.lockState = CursorLockMode.Confined;

                mousePos = Input.mousePosition;
            }
            else if (ctx.canceled)
            {
                this.Inventory.ItemWheel.SelectItem(mousePos, this);
                itemWheel = false;
                Cursor.lockState = CursorLockMode.Locked;
                if (this.Inventory.ItemWheel.IsSwitchingItem)
                    Inventory.ChangeMode(PlayerControllers.Inventory.Inventory.Mode.Item);
                else
                    Inventory.ChangeMode(PlayerControllers.Inventory.Inventory.Mode.Weapon);
            }
        }

        public void OnAbilityTreeOpened(InputAction.CallbackContext ctx)
        {
            if (!IsOwner || !ctx.performed)
                return;

            if (playerState != PlayerState.Normal)
            {
                return;
            }
            
            if (playerState == PlayerState.InAbilityTree)
            {
                HUDController.Singleton.HideAbilityTree();
            }
            else
            {
                HUDController.Singleton.ShowAbilityTree();
            }
        }

        public void OnEscape(InputAction.CallbackContext ctx)
        {
            if (!IsOwner || !ctx.performed)
                return;

            if (playerState == PlayerState.InAbilityTree)
            {
                HUDController.Singleton.HideAbilityTree();
                return;
            }

            if (playerState == PlayerState.Escaped)
            {
                HUDController.Singleton.HideEscapeMenu();
            }
            else
            {
                HUDController.Singleton.ShowEscapeMenu();
            }
        }
    }
}