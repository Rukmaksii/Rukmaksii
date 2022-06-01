using System.Collections.Generic;
using GameScene.GameManagers;
using GameScene.Map;
using GameScene.Minions;
using UnityEngine;
using UnityEngine.UI;

namespace GameScene.HUD
{
    public partial class HUDController : MonoBehaviour
    {
        [SerializeField] protected GameObject map;
        [SerializeField] protected Image arrow;
        [SerializeField] protected GameObject pointParent;
        [SerializeField] protected GameObject redPointPrefab;
        [SerializeField] protected GameObject bluePointPrefab;
        [SerializeField] protected Image objOverlay1;
        [SerializeField] protected Image objOverlay2;
        [SerializeField] protected Image objOverlay3;

        private Vector3 _mapLoc;
        private const float MapRatio = 385f/1000;
        private List<GameObject> redPoints = new List<GameObject>();
        private List<GameObject> bluePoints = new List<GameObject>();

        private void UpdateMonster()
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
                redPoints.Add(Instantiate(redPointPrefab, pointParent.transform, true));
            }
            
            Vector3 playerPosition = GameController.Singleton.LocalPlayer.transform.localPosition;
            
            for (int i = 0; i < monsterList.Length; i++)
            {
                if (Vector3.Distance(monsterList[i].transform.localPosition, playerPosition) < 50)
                {
                    GameObject point = redPoints[i];
                    Vector3 monsterPos = monsterList[i].transform.localPosition;
                    point.transform.localPosition = new Vector3(monsterPos.x * MapRatio, monsterPos.z * MapRatio, 0);
                }
            }
        }
        
        private void UpdateMinion()
        {
            GameObject[] minionList = GameObject.FindGameObjectsWithTag("Minion");

            GameObject[] oldBluePoints = GameObject.FindGameObjectsWithTag("BluePoint");
            foreach (GameObject point in oldBluePoints)
            {
                Destroy(point);
            }
            bluePoints.Clear();
            
            foreach (GameObject monster in minionList)
            {
                bluePoints.Add(Instantiate(bluePointPrefab, pointParent.transform, true));
            }
            
            Vector3 playerPosition = GameController.Singleton.LocalPlayer.transform.localPosition;
            
            for (int i = 0; i < minionList.Length; i++)
            {
                BaseMinion compo = minionList[i].GetComponent<BaseMinion>();
                if (compo != null && compo.TeamId == GameController.Singleton.LocalPlayer.TeamId && Vector3.Distance(minionList[i].transform.localPosition, playerPosition) < 50)
                {
                    GameObject point = bluePoints[i];
                    Vector3 minionPos = minionList[i].transform.localPosition;
                    point.transform.localPosition = new Vector3(minionPos.x * MapRatio, minionPos.z * MapRatio, 0);
                }
            }
        }
        
        private void UpdateMap()
        {
            Vector3 playerPosition = GameController.Singleton.LocalPlayer.transform.localPosition;
            arrow.transform.localPosition = new Vector3(playerPosition.x * MapRatio, playerPosition.z * MapRatio, 0);
            
            Quaternion camRotation = GameObject.FindGameObjectWithTag("Player Camera").transform.localRotation;
            arrow.transform.localRotation = new Quaternion(0, 0, -camRotation.y, camRotation.w);
            
            UpdateObjectives();
            UpdateMonster();
            UpdateMinion();
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
            else if (obj1.canCapture)
                objOverlay1.color = Color.black;
            else
                objOverlay1.color = Color.white;
            
            if (obj2.CurrentState == ObjectiveController.State.Captured && obj2.CapturingTeam == 0)
                objOverlay2.color = Color.blue;
            else if (obj2.CurrentState == ObjectiveController.State.Captured && obj1.CapturingTeam == 1)
                objOverlay2.color = Color.red;
            else if (obj2.canCapture)
                objOverlay2.color = Color.black;
            else
                objOverlay2.color = Color.white;
            
            if (obj3.CurrentState == ObjectiveController.State.Captured && obj3.CapturingTeam == 0)
                objOverlay3.color = Color.blue;
            else if (obj3.CurrentState == ObjectiveController.State.Captured && obj1.CapturingTeam == 1)
                objOverlay3.color = Color.red;
            else if (obj3.canCapture)
                objOverlay3.color = Color.black;
            else
                objOverlay3.color = Color.white;
        }

        public void ScaleUp()
        {
            pointParent.transform.localPosition = new Vector3(0, 0, 0);
            pointParent.transform.localScale = new Vector3(2.6f, 2.6f, 1);
        }
    }
}