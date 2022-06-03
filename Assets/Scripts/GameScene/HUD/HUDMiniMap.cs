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
        [SerializeField] private Sprite capLock;
        [SerializeField] private Sprite capOpen;

        private Vector3 _mapLoc;
        private const float MapRatio = 385f/1000;
        private List<GameObject> redPoints = new List<GameObject>();
        private List<GameObject> bluePoints = new List<GameObject>();

        private Color colorT1 = Color.blue;
        private Color colorT2 = Color.red;
        private Color colorNeutral = Color.gray;
        private Color colorOpen = Color.green;


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
            ObjectiveController obj0 = GameObject.Find("Objective0").GetComponentInChildren<ObjectiveController>();
            ObjectiveController obj1 = GameObject.Find("Objective1").GetComponentInChildren<ObjectiveController>();
            ObjectiveController obj2 = GameObject.Find("Objective2").GetComponentInChildren<ObjectiveController>();

            if (obj0.CurrentState == ObjectiveController.State.Captured && obj0.CapturingTeam == 0)
                objOverlay1.color = colorT1;
            else if (obj0.CurrentState == ObjectiveController.State.Captured && obj0.CapturingTeam == 1)
                objOverlay1.color = colorT2;
            else if (obj0.CanCapture)
                objOverlay1.color = colorOpen;
            else
                objOverlay1.color = colorNeutral;

            if (obj2.CurrentState == ObjectiveController.State.Captured && obj2.CapturingTeam == 0)
                objOverlay2.color = colorT1;
            else if (obj2.CurrentState == ObjectiveController.State.Captured && obj0.CapturingTeam == 1)
                objOverlay2.color = colorT2;
            else if (obj2.CanCapture)
                objOverlay2.color = colorOpen;
            else
                objOverlay2.color = colorNeutral;
            
            if (obj1.CurrentState == ObjectiveController.State.Captured && obj1.CapturingTeam == 0)
                objOverlay3.color = colorT1;
            else if (obj1.CurrentState == ObjectiveController.State.Captured && obj0.CapturingTeam == 1)
                objOverlay3.color = colorT2;
            else if (obj1.CanCapture)
                objOverlay3.color = colorOpen;
            else
                objOverlay3.color = colorNeutral;
            
            if (objOverlay1.color == colorOpen)
                objOverlay1.sprite = capOpen;
            else
                objOverlay1.sprite = capLock;
            
            if (objOverlay2.color == colorOpen)
                objOverlay2.sprite = capOpen;
            else
                objOverlay2.sprite = capLock;
            
            if (objOverlay3.color == colorOpen)
                objOverlay3.sprite = capOpen;
            else
                objOverlay3.sprite = capLock;
        }

        public void ScaleUp(bool up)
        {
            if (up)
            {
                pointParent.transform.localPosition = new Vector3(0, 0, 0);
                pointParent.transform.localScale = new Vector3(2.6f, 2.6f, 1);
            }
            else
            {
                pointParent.transform.localPosition = _mapLoc;
                pointParent.transform.localScale = new Vector3(1, 1, 1);
            }
        }
    }
}