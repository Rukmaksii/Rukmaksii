using model;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace GameScene.Menus
{
    public class StartMenuHandler : MonoBehaviour
    {
        [SerializeField] private GameObject mainMenu;
        [SerializeField] private GameObject selectMenu;
        [SerializeField] private GameObject serverMenu;

        [SerializeField] private Dropdown chosenClass;
        [SerializeField] private Dropdown chosenTeam;

        [SerializeField] private ConnectionScriptableObject connectionData;

        // Start is called before the first frame update
        void Start()
        {
            selectMenu.SetActive(false);
            serverMenu.SetActive(false);
        }

        // Update is called once per frame
        void Update()
        {
        }


        public void OnPlay()
        {
            switch (chosenClass.value)
            {
                case 0:
                    connectionData.Data.ClassName = "test class";
                    break;
                default:
                    connectionData.Data.ClassName = "test class";
                    break;
            }

            connectionData.Data.TeamId = chosenTeam.value;

            SceneManager.LoadScene("GameScene");
        }

        public void OnSingleplayer()
        {
            connectionData.Data = new ConnectionData
            {
                ConnectionType = "host",
                TeamId = 0
            };

            SceneManager.LoadScene("LobbyScene");

            mainMenu.SetActive(false);
            selectMenu.SetActive(true);
        }

        public void OnMultiplayer()
        {
            connectionData.Data = new ConnectionData();

            connectionData.Data.ConnectionType = "client";

            SceneManager.LoadScene("LobbyScene");

            mainMenu.SetActive(false);
            selectMenu.SetActive(true);
        }

        public void OnServer()
        {
            connectionData.Data = new ConnectionData();

            connectionData.Data.ConnectionType = "server";

            mainMenu.SetActive(false);
            OnPlay();
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