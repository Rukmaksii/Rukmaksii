using GameManagers;
using Map;
using PlayerControllers;
using UnityEngine;
using UnityEngine.UI;

namespace HUD
{
    public partial class HUDController
    {
        [SerializeField] protected Image capturingState;
        
        private ObjectiveController _capturePoint;
        
        /**
         * <summary>displays and hides the circle for capturing an objective</summary>
         * <param name="area">ObjectiveController for the objective being captured</param>
         * <param name="player">BasePlayer for the player capturing it</param>
         * <param name="state">bool for whether the player enters or leaves the objective</param>
         */
        public void DisplayCaptureState(ObjectiveController area, BasePlayer player, bool state)
        {
            if (player == GameController.Singleton.LocalPlayer)
            {
                if (state)
                {
                    _capturePoint = area;
                    capturingState.enabled = true;
                }
                else
                {
                    _capturePoint = null;
                    capturingState.enabled = false;
                }
            }
        }

        private void SetCapIconColor()
        {
            if (_capturePoint.CapturingTeam == 1)
                capturingState.color = Color.red;
            else if (_capturePoint.CapturingTeam == 0)
                capturingState.color = Color.blue;
        }
    }
}