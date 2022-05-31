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
        private bool active = true;

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
                    basePosition = Mouse.current.position.ReadValue();
                    transform.position = basePosition;
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
            if (!active)
                return;
            PointerEventData ev = new PointerEventData(EventSystem.current);
            ev.position = Mouse.current.position.ReadValue();
            List<RaycastResult> results = new List<RaycastResult>();
            raycaster.Raycast(ev, results);

            bool foundElement = false;

            foreach (var child in GetComponentsInChildren<RectTransform>())
            {
                if (child.parent != transform)
                    continue;

                Debug.Log(child);


                RectTransform foundChild = null;
                for (int i = 0; !foundElement && i < results.Count && foundChild == null; i++)
                {
                    if (results[i].gameObject.GetComponent<RectTransform>().parent == child.transform ||
                        results[i].gameObject == child.gameObject)
                    {
                        foundChild = child;
                    }
                }

                Vector3 scaleDirection = child.localPosition.normalized;
                float scaleFactor = 1.3f;

                if (foundChild != null)
                {
                    foundElement = true;
                    strategy = foundChild.name.ToLower() switch
                    {
                        "protect" => IMinion.Strategy.PROTECT,
                        "attack" => IMinion.Strategy.ATTACK,
                        "defend" => IMinion.Strategy.DEFEND,
                        _ => strategy
                    };
                    foundChild.localScale = new Vector3(1.3f, 1.3f, 1);
                }
                else
                {
                    child.localScale = new Vector3(1f, 1f, 1);
                    strategy = IMinion.Strategy.Count;
                }
            }
        }
    }
}