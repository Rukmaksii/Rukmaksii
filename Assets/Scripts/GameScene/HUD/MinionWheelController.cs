using GameScene.model;
using UnityEngine;
using UnityEngine.EventSystems;

namespace GameScene.HUD
{
    public class MinionWheelController : MonoBehaviour
    {
        private bool active = false;

        public IMinion.Strategy strategy { get; private set; }

        private Vector2 basePosition = Vector2.zero;

        public bool IsActive
        {
            get => active;
            set
            {
                if (value)
                {
                    gameObject.SetActive(false);
                    // Activate(Mouse.current.position.ReadValue());
                }
                else
                {
                    gameObject.SetActive(false);
                }
            }
        }

        // Start is called before the first frame update
        void Start()
        {
            foreach (var child in gameObject.GetComponentsInChildren<RectTransform>())
            {
                if (child.parent != transform)
                    continue;
            }
        }

        // Update is called once per frame
        // ReSharper disable Unity.PerformanceAnalysis
        void Update()
        {
        }

        private void Activate(Vector2 position)
        {
        }


        public void OnPointerEnter(PointerEventData eventData)
        {
            Debug.Log($"entering {eventData.hovered.Count}");
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            Debug.Log($"exiting {eventData.hovered.Count}");
        }
    }
}