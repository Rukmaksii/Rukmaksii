using System.Diagnostics;
using GameManagers;
using model;
using UnityEngine;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;

namespace Menus
{
    public class EndScreen : MonoBehaviour
    {
        [SerializeField] private Text winner;

        // Start is called before the first frame update
        void Start()
        {
            Cursor.lockState = CursorLockMode.Confined;
        }

        // Update is called once per frame
        void Update()
        {
        }
        
        public void OnQuit()
        {
            Application.Quit();
        }
    }
}