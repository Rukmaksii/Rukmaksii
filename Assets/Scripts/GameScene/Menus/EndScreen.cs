using UnityEngine;
using UnityEngine.UI;

namespace GameScene.Menus
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