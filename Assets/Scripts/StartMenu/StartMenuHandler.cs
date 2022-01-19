using System.Diagnostics;
using GameManagers;
using model;
using UnityEngine;
using UnityEngine.UI;

namespace StartMenu
{
    public class StartMenuHandler : MonoBehaviour
    {
        [SerializeField] private Dropdown connectionType;
        [SerializeField] private Dropdown chosenClass;

        [SerializeField] private ConnectionScriptableObject connectionData;

        // Start is called before the first frame update
        void Start()
        {
        }

        // Update is called once per frame
        void Update()
        {
        }

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
    }
}