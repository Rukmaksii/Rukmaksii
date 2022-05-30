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

        [SerializeField] private Dropdown chosenClass;
        [SerializeField] private Dropdown chosenTeam;

        [SerializeField] private ConnectionScriptableObject connectionData;

        // Start is called before the first frame update
        void Start()
        {
            selectMenu.SetActive(false);
        }

        // Update is called once per frame
        void Update()
        {
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
            connectionData.Data = new ConnectionData
            {
                ConnectionType = "client"
            };

            SceneManager.LoadScene("LobbyScene");

            mainMenu.SetActive(false);
            selectMenu.SetActive(true);
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