using System;
using System.Collections.Generic;
using GameScene.GameManagers;
using GameScene.PlayerControllers.BasePlayer;
using LobbyScene;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace GameScene.HUD
{
    public class PseudoController : MonoBehaviour
    {
        [SerializeField] private GameObject pseudoHolder;
        [SerializeField] private float renderDistance = 20f;
        [SerializeField] private Color teamColor;
        [SerializeField] private Color enemyColor;


        private Dictionary<ulong, GameObject> pseudoDictionary = new Dictionary<ulong, GameObject>();

        private void Start()
        {
            NetworkManager.Singleton.OnClientDisconnectCallback +=
                delegate(ulong clientId)
                {
                    if (clientId != NetworkManager.ServerClientId)
                        Destroy(pseudoDictionary[clientId]);
                };
        }

        // Update is called once per frame
        void Update()
        {
            var localPlayer = GameController.Singleton.LocalPlayer;
            if (localPlayer == null)
                return;

            foreach (var player in GameController.Singleton.Players)
            {
                if (player.OwnerClientId == localPlayer.OwnerClientId)
                    continue;

                if (!pseudoDictionary.ContainsKey(player.OwnerClientId))
                {
                    var holder = Instantiate(pseudoHolder, gameObject.transform).GetComponent<RectTransform>();
                    pseudoDictionary[player.OwnerClientId] = holder.gameObject;
                    holder.anchorMin = Vector2.zero;
                    holder.anchorMax = Vector2.zero;
                }

                // ReSharper disable twice Unity.InefficientPropertyAccess
                bool shouldDisplay =
                    Vector3.Dot(localPlayer.CameraController.transform.forward,
                        player.transform.position - localPlayer.transform.position) > 0 &&

                    Vector3.Distance(player.transform.position, localPlayer.transform.position) <=
                    renderDistance && player.CurrentHealth > 0;
                GameObject obj = pseudoDictionary[player.OwnerClientId];
                obj.SetActive(shouldDisplay);
                if (shouldDisplay)
                {
                    var textObj = obj.GetComponentInChildren<Text>();
                    textObj.text = GetPseudo(player.OwnerClientId);
                    obj.GetComponent<RectTransform>().anchoredPosition = PseudoPosition(player);
                    var color = player.TeamId == 0 ? teamColor : enemyColor;
                    textObj.color = color;
                }
            }
        }

        /**
         * <returns>the pseudo associated with the given client id</returns>
         */
        private string GetPseudo(ulong id)
        {
            string pseudo = LobbyManager.Singleton.PlayersRegistry[id].Pseudo;
            return String.IsNullOrEmpty(pseudo) ? $"Player {id}" : pseudo;
        }

        private Vector2 PseudoPosition(BasePlayer player)
        {
            var hud = GetComponent<HUDController>();
            Vector2 scalars = GameController
                .Singleton
                .LocalPlayer
                .CameraController
                .Camera
                .WorldToViewportPoint(
                    player.PseudoPosition
                );
            return new Vector2(scalars.x * hud.CanvasWidth, scalars.y * hud.CanvasHeight);
        }
    }
}