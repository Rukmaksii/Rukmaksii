using System.Collections.Generic;
using GameManagers;
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
    }
}