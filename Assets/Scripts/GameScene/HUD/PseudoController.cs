using System;
using System.Collections.Generic;
using GameScene.GameManagers;
using LobbyScene;
using Unity.Netcode;
using UnityEngine;

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
                pseudoDictionary[player.OwnerClientId].SetActive(shouldDisplay);
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
    }
}