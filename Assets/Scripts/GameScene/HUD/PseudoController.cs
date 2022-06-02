using System;
using System.Collections.Generic;
using GameScene.GameManagers;
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


        private Dictionary<ulong, GameObject> pseudoDictionary = new Dictionary<ulong, GameObject>();

        // Update is called once per frame
        void Update()
        {
            var localPlayer = GameController.Singleton.LocalPlayer;
            if (localPlayer == null)
                return;

            foreach (var player in GameController.Singleton.Players)
            {
                if (player.OwnerClientId == NetworkManager.Singleton.LocalClientId)
                    continue;

                if (!pseudoDictionary.ContainsKey(player.OwnerClientId))
                    pseudoDictionary[player.OwnerClientId] = Instantiate(pseudoHolder, gameObject.transform);


                bool shouldDisplay =
                    Vector3.Distance(player.transform.position, localPlayer.transform.position) <=
                    renderDistance;
                GameObject obj = pseudoDictionary[player.OwnerClientId];
                obj.SetActive(shouldDisplay);
                if (shouldDisplay)
                {
                    obj.GetComponentInChildren<Text>().text = GetPseudo(player.OwnerClientId);
                    obj.GetComponent<RectTransform>().anchoredPosition = PseudoPosition(player.gameObject);
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

        private Vector2 PseudoPosition(GameObject obj)
        {
            var hud = GetComponent<HUDController>();
            Vector2 scalars = GameController
                    .Singleton
                    .LocalPlayer
                    .CameraController
                    .Camera
                    .WorldToViewportPoint(
                        obj.transform.position
                    );
            Debug.Log(scalars);
            return new Vector2(scalars.x * hud.CanvasWidth, scalars.y * hud.CanvasHeight);
        }
    }
}