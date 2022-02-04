using System.Diagnostics;
using GameManagers;
using model;
using UnityEngine;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;

namespace Menus
{
    public class StartMenuHandler : MonoBehaviour
    {
        [SerializeField] private GameObject mainMenu;
        [SerializeField] private GameObject selectMenu;
        
        [SerializeField] private Dropdown chosenClass;

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

        /*
        public void OnPlay()
        {
            connectionData.Data = new ConnectionData();

            // TODO : set a correct implementation for class choice
            switch (chosenClass.value)
            {
                case 0:
                    connectionData.Data.ClassName = "test class";
                    break;
                default:
                    connectionData.Data.ClassName = "test class";
                    break;
            }

            // TODO : safen connection type choice
            switch (connectionType.value)
            {
                case 0:
                    connectionData.Data.ConnectionType = "host";
                    break;
                case 1:
                    connectionData.Data.ConnectionType = "client";
                    break;
                default:
                    connectionData.Data.ConnectionType = "server";
                    break;
            }


            UnityEngine.SceneManagement.SceneManager.LoadScene("GameScene");
        }
        */

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
            UnityEngine.SceneManagement.SceneManager.LoadScene("GameScene");
        }
        
        public void OnSingleplayer()
        {
            connectionData.Data = new ConnectionData();
            
            connectionData.Data.ConnectionType = "host";
            
            mainMenu.SetActive(false);
            selectMenu.SetActive(true);
        }
        
        public void OnMultiplayer()
        {
            connectionData.Data = new ConnectionData();
            
            connectionData.Data.ConnectionType = "client";
            
            mainMenu.SetActive(false);
            selectMenu.SetActive(true);
        }
        
        public void OnOptions()
        {
            // TODO
            Debug.Log("Entering options menu");
        }
        
        public void OnQuit()
        {
            Debug.Log("Quitting the game");
            Application.Quit();
        }
    }
}