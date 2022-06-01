using System;
using System.Linq;
using model;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = System.Random;

namespace GameScene.Menus
{
    public class StartMenuHandler : MonoBehaviour
    {
        [SerializeField] private GameObject mainMenu;

        [SerializeField] private InputField nicknameInput;
        [SerializeField] private InputField roomInput;

        [SerializeField] private ConnectionScriptableObject connectionData;

        // Start is called before the first frame update
        void Start()
        {
        }

        // Update is called once per frame
        void Update()
        {
        }

        private string GenerateRoomName()
        {
            Random rnd = new Random();
            string str = "";
            for (int i = 0; i < 5; i++)
                str += (char) rnd.Next(65, 91);
            return str;
        }
        
        public void OnCreateRoom()
        {
            if (String.IsNullOrEmpty(nicknameInput.text))
                return;

            connectionData.Data = new ConnectionData
            {
                Pseudo = nicknameInput.text,
                ConnectionType = "host",
                TeamId = 0,
                RoomName = GenerateRoomName()
            };

            SceneManager.LoadScene("LobbyScene");

            mainMenu.SetActive(false);
        }

        public void OnJoinRoom()
        {
            if (String.IsNullOrEmpty(nicknameInput.text) || String.IsNullOrEmpty(roomInput.text))
                return;
            
            connectionData.Data = new ConnectionData
            {
                Pseudo = nicknameInput.text,
                ConnectionType = "client",
                RoomName =  roomInput.text
            };

            SceneManager.LoadScene("LobbyScene");

            mainMenu.SetActive(false);
        }


        public void OnOptions()
        {
            // TODO: add an option tab
            Debug.Log("Entering options menu");
        }

        public void OnQuit()
        {
            Debug.Log("Quitting the game");
            Application.Quit();
        }
    }
}