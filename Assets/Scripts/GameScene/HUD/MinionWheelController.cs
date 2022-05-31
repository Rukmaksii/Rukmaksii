using System.Collections.Generic;
using GameScene.model;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace GameScene.HUD
{
    [RequireComponent(typeof(GraphicRaycaster))]
    public class MinionWheelController : MonoBehaviour
    {
        private bool active = false;

        public IMinion.Strategy strategy { get; private set; }

        private Vector2 basePosition = Vector2.zero;

        private GraphicRaycaster raycaster;

        public bool IsActive
        {
            get => active;
            set
            {
                gameObject.SetActive(value);
                GetComponent<Canvas>().enabled = value;
                active = value;
                if (value)
                {
                    Activate(Mouse.current.position.ReadValue());
                }
            }
        }

        // Start is called before the first frame update
        void Start()
        {
            raycaster = GetComponent<GraphicRaycaster>();
        }

        // Update is called once per frame
        // ReSharper disable Unity.PerformanceAnalysis
        void Update()
        {
            PointerEventData ev = new PointerEventData(EventSystem.current);
            ev.position = Mouse.current.position.ReadValue();
            List<RaycastResult> results = new List<RaycastResult>();
            raycaster.Raycast(ev, results);

            bool foundElement = false;

            foreach (var child in GetComponentsInChildren<RectTransform>())
            {
                if (child.parent != transform)
                    continue;


                RectTransform foundChild = null;
                for (int i = 0; !foundElement && i < results.Count && foundChild == null; i++)
                {
                    if (results[i].gameObject.GetComponent<RectTransform>().parent == child.transform ||
                        results[i].gameObject == child.gameObject)
                    {
                        foundChild = child;
                    }
                }

                if (foundChild != null)
                {
                    foundChild.localScale = new Vector3(1.3f, 1.3f, 0);
                    foundElement = true;
                }
                else
                {
                    child.localScale = new Vector3(1, 1, 1);
                }
            }
        }

        private void Activate(Vector2 position)
        {
        }
    }
}