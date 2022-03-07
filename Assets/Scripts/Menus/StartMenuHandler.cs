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