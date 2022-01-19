using UnityEngine;
using UnityEngine.UI;

namespace StartMenu
{
    public class StartMenuHandler : MonoBehaviour
    {


        [SerializeField] private Dropdown connectionType;
        [SerializeField] private Dropdown chosenClass;

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
            Debug.Log($"class: {chosenClass.itemText.text}, connection type: {connectionType.itemText.text}");
            UnityEngine.SceneManagement.SceneManager.LoadScene("GameScene");
        }
    }
}
