using Unity.Netcode;
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Map;
using MonstersControler;
using PlayerControllers;
using UnityEditor;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace GameManagers
{
    public class Gameloop : NetworkBehaviour
    {
        private NetworkVariable<DateTime> referenceTime = new NetworkVariable<DateTime>();
        private NetworkVariable<DateTime> currTime = new NetworkVariable<DateTime>();

        private int objectiveDelay;
        private bool hasBeenChange;
        private int numberOfMonster;

        private GameObject[] captureArea;
        
        private ShieldController shield1;
        private ShieldController shield2;

        public static List<MonsterController> ListOfMonster;

        // Start is called before the first frame update
        void Start()
        {
            numberOfMonster = 4;
            ListOfMonster = new List<MonsterController>();
            if (NetworkManager.Singleton.IsServer)
            {
                referenceTime.Value = DateTime.Now;
            }
            shield1 = GameObject.Find("Shield1").GetComponent<ShieldController>();
            shield2 = GameObject.Find("Shield2").GetComponent<ShieldController>();
            objectiveDelay = 5;
            hasBeenChange = false;
        }

        // Update is called once per frame
        void Update()
        {
            captureArea = GameObject.FindGameObjectsWithTag("CaptureArea");
            if (NetworkManager.Singleton.IsServer)
            {
                //set the current time
                currTime.Value = DateTime.Now;
                //check if there are the right number of monster 
                if(ListOfMonster.Count < numberOfMonster)
                    SpawnMonsters(numberOfMonster - ListOfMonster.Count);
            }
            //deactivate the collision between each player and it's team's shield
            foreach (BasePlayer player in GameController.Singleton.Players)
            {
                Collider[] child = player.gameObject.GetComponentsInChildren<Collider>();
                if (player.TeamId == shield1.TeamId)
                {
                    foreach (Collider colliderchild in child)
                    {
                        Physics.IgnoreCollision(colliderchild, shield1.gameObject.GetComponent<MeshCollider>());
                    }
                    Physics.IgnoreCollision(player.gameObject.GetComponent<Collider>(), shield1.gameObject.GetComponent<MeshCollider>());
                }
                else
                {
                    foreach (Collider colliderchild in child)
                    {
                        Physics.IgnoreCollision(colliderchild, shield2.gameObject.GetComponent<MeshCollider>());
                    }
                    Physics.IgnoreCollision(player.gameObject.GetComponent<Collider>(), shield2.gameObject.GetComponent<MeshCollider>());
                }
            }
            //create the timer
            var timer = currTime.Value - referenceTime.Value;
            //Debug.Log($"{timer.Hours}:{timer.Minutes}:{timer.Seconds}");
            if (timer.Minutes % objectiveDelay == 0 && timer.Seconds == 0 && !hasBeenChange)
            {
                ChangeCapturePoints();
                hasBeenChange = true;
                StartCoroutine(Wait1Second());
            }
            //for each capture point it checks if one of them is catured if so, it deactivates the ennemy's shield
            foreach (GameObject area in captureArea)
            {
                ObjectiveController objective = area.GetComponent<ObjectiveController>();
                if (objective.CurrentState is ObjectiveController.State.Captured)
                {
                    if (objective.CapturingTeam != shield1.TeamId)
                    {
                        //Debug.Log("shield 1 deactivated");
                        shield1.Activated.Value = false;
                        shield2.Activated.Value = true;
                    }
                    else
                    {
                        //Debug.Log("shield 2 deactivated");
                        shield1.Activated.Value = true;
                        shield2.Activated.Value = false;
                    }
                }
            }
            //Debug.Log(ListOfMonster.Count);
        }


        public void ChangeCapturePoints()
        {
            foreach (GameObject area in captureArea)
            {
                area.GetComponent<ObjectiveController>().ToggleCanCapture(false);
            }
            captureArea[Random.Range(0, captureArea.Length)]
                .GetComponent<ObjectiveController>()
                .ToggleCanCapture(true);
            //Activate all the shields
            shield1.Activated.Value = true;
            shield2.Activated.Value = true;
        }
        public void PossibleJoingame()
        {

        }

        public void SpawnMonsters(int number)
        {
            for (int i = 0; i < number; i++)
            {
                Vector3 pos = new Vector3(Random.Range(-115, 194), 10, Random.Range(-153, 138));
                if (Physics.Raycast(pos, Vector3.down, out RaycastHit hit))
                {
                    GameObject instance = Instantiate(GameController.Singleton.MonsterPrefab, hit.point, Quaternion.identity);
                    instance.GetComponent<NetworkObject>().Spawn();
                }
            }
        }

        IEnumerator Wait1Second()
        {
            yield return new WaitForSeconds(1);
            hasBeenChange = false;
        }
    }
}

