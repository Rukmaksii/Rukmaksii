using Unity.Netcode;
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Map;
using MonstersControler;
using PlayerControllers;
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
            if (NetworkManager.Singleton.IsServer || NetworkManager.Singleton.IsHost)
            {
                referenceTime.Value = DateTime.Now;
                SpawnMonsters(numberOfMonster);
            }
            shield1 = GameObject.Find("Shield1").GetComponent<ShieldController>();
            shield2 = GameObject.Find("Shield2").GetComponent<ShieldController>();
            objectiveDelay = 5;
            hasBeenChange = false;
            foreach (BasePlayer player in GameController.Singleton.Players)
            {
                Collider[] child = player.GetComponentsInChildren<Collider>();
                if (player.TeamId == shield1.TeamId)
                {
                    foreach (Collider colliderchild in child)
                    {
                        Physics.IgnoreCollision(colliderchild, GameObject.Find("Shield1").GetComponent<MeshCollider>());
                    }
                    Physics.IgnoreCollision(player.GetComponent<Collider>(), GameObject.Find("Shield1").GetComponent<MeshCollider>());
                }
                else
                {
                    foreach (Collider colliderchild in child)
                    {
                        Physics.IgnoreCollision(colliderchild, GameObject.Find("Shield2").GetComponent<MeshCollider>());
                    }
                    Physics.IgnoreCollision(player.GetComponent<Collider>(), GameObject.Find("Shield2").GetComponent<MeshCollider>());
                }
            }
        }

        // Update is called once per frame
        void Update()
        {
            captureArea = GameObject.FindGameObjectsWithTag("CaptureArea");
            if (NetworkManager.Singleton.IsServer || NetworkManager.Singleton.IsHost)
            {
                currTime.Value = DateTime.Now;
                if(ListOfMonster.Count < numberOfMonster)
                    SpawnMonsters(numberOfMonster - ListOfMonster.Count);
            }
            var timer = currTime.Value - referenceTime.Value;
            if (timer.Minutes % objectiveDelay == 0 && timer.Seconds == 0 && !hasBeenChange)
            {
                ChangeCapturePoints();
                hasBeenChange = true;
                StartCoroutine(Wait1Second());
            }
            foreach (GameObject area in captureArea)
            {
                ObjectiveController objective = area.GetComponent<ObjectiveController>();
                if (objective.CurrentState is ObjectiveController.State.Captured)
                {
                    if (objective.CapturingTeam != shield1.TeamId)
                    {
                        shield1.Activated.Value = false;
                        shield2.Activated.Value = true;
                    }
                    else
                    {
                        shield1.Activated.Value = true;
                        shield2.Activated.Value = false;
                    }
                }
                else
                {
                    shield1.Activated.Value = true;
                    shield2.Activated.Value = true;
                }
            }
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

