using System.Collections.Generic;
using GameManagers;
using Map;
using UnityEngine;
using UnityEngine.UI;

namespace HUD
{
    public partial class HUDController : MonoBehaviour
    {
        [SerializeField] protected GameObject map;
        [SerializeField] protected Image arrow;
        [SerializeField] protected GameObject redPointParent;
        [SerializeField] protected GameObject redPointPrefab;
        [SerializeField] protected Image objOverlay1;
        [SerializeField] protected Image objOverlay2;
        [SerializeField] protected Image objOverlay3;

        private List<GameObject> redPoints = new List<GameObject>();
        
        private void UpdateMapMonsters()
        {
            GameObject[] monsterList = GameObject.FindGameObjectsWithTag("Monster");

            GameObject[] oldRedPoints = GameObject.FindGameObjectsWithTag("RedPoint");
            foreach (GameObject point in oldRedPoints)
            {
                Destroy(point);
            }
            redPoints.Clear();
            
            foreach (GameObject monster in monsterList)
            {
                redPoints.Add(Instantiate(redPointPrefab, redPointParent.transform, true));
            }
            
            Vector3 playerPosition = GameController.Singleton.LocalPlayer.transform.localPosition;
            
            for (int i = 0; i < monsterList.Length; i++)
            {
                if (Vector3.Distance(monsterList[i].transform.localPosition, playerPosition) < 50)
                {
                    GameObject point = redPoints[i];
                    Vector3 monsterPos = monsterList[i].transform.position;
                    point.transform.localPosition = new Vector3(monsterPos.x - 55, monsterPos.z + 15, 0);
                }
            }
        }
        
        private void UpdateMap()
        {
            Vector3 playerPosition = GameController.Singleton.LocalPlayer.transform.localPosition;
            arrow.transform.localPosition = new Vector3(playerPosition.x - 40, playerPosition.z + 15, 0);
            
            Quaternion camRotation = GameObject.FindGameObjectWithTag("Player Camera").transform.localRotation;
            arrow.transform.localRotation = new Quaternion(0, 0, -camRotation.y, camRotation.w);
        }

        private void UpdateObjectives()
        {
            ObjectiveController obj1 = GameObject.Find("Objective1").GetComponentInChildren<ObjectiveController>();
            ObjectiveController obj3 = GameObject.Find("Objective2").GetComponentInChildren<ObjectiveController>();
            ObjectiveController obj2 = GameObject.Find("Objective3").GetComponentInChildren<ObjectiveController>();

            if (obj1.CurrentState == ObjectiveController.State.Captured && obj1.CapturingTeam == 0)
                objOverlay1.color = Color.blue;
            else if (obj1.CurrentState == ObjectiveController.State.Captured && obj1.CapturingTeam == 1)
                objOverlay1.color = Color.red;
            else
                objOverlay1.color = Color.white;
            
            if (obj2.CurrentState == ObjectiveController.State.Captured && obj2.CapturingTeam == 0)
                objOverlay2.color = Color.blue;
            else if (obj2.CurrentState == ObjectiveController.State.Captured && obj1.CapturingTeam == 1)
                objOverlay2.color = Color.red;
            else
                objOverlay2.color = Color.white;
            
            if (obj3.CurrentState == ObjectiveController.State.Captured && obj3.CapturingTeam == 0)
                objOverlay3.color = Color.blue;
            else if (obj3.CurrentState == ObjectiveController.State.Captured && obj1.CapturingTeam == 1)
                objOverlay3.color = Color.red;
            else
                objOverlay3.color = Color.white;
        }
    }
}