using System;
using System.Collections;
using System.Collections.Generic;
using ExitGames.Client.Photon.StructWrapping;
using GameScene.HUD;
using GameScene.Map;
using GameScene.Monster;
using GameScene.PlayerControllers.BasePlayer;
using Unity.Mathematics;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace GameScene.GameManagers
{
    public class Gameloop : NetworkBehaviour
    {
        private NetworkVariable<DateTime> referenceTime = new NetworkVariable<DateTime>();
        private NetworkVariable<DateTime> currTime = new NetworkVariable<DateTime>();

        private int objectiveDelay;
        private bool hasBeenChange;
        private const int numberOfMonster = 20;

        [SerializeField]private GameObject[] captureArea;

        private ShieldController shield1;
        private ShieldController shield2;
        private BaseController base1;
        private BaseController base2;

        private int monsterCount = 0;
        private bool deactivateAllCapturePoints = false;
        private int timeToEnd = 20;

        public static Gameloop Singleton { get; private set; }

        private void Awake()
        {
            if (Singleton != null && Singleton != this)
            {
                Destroy(this);
                return;
            }

            Singleton = this;
        }

        // Start is called before the first frame update
        void Start()
        {
            if (NetworkManager.Singleton.IsServer)
            {
                referenceTime.Value = DateTime.Now;
            }

            shield1 = GameObject.Find("Shield1").GetComponent<ShieldController>();
            shield2 = GameObject.Find("Shield2").GetComponent<ShieldController>();
            base1 = GameObject.Find("Base1").GetComponent<BaseController>();
            base2 = GameObject.Find("Base2").GetComponent<BaseController>();
            objectiveDelay = 5;
            hasBeenChange = false;
        }

        // Update is called once per frame
        void Update()
        {
            if (IsOwner)
            {
                if (shield1 != null && shield1.TeamId != 0)
                    shield1.UpdateTeamServerRpc(0);
                if (shield2 != null && shield2.TeamId != 1)
                    shield2.UpdateTeamServerRpc(1);
            }
            
            //captureArea = GameObject.FindGameObjectsWithTag("CaptureArea");
            if (NetworkManager.Singleton.IsServer)
            {
                //set the current time
                currTime.Value = DateTime.Now;
                //check if there are the right number of monster 
                if (monsterCount < numberOfMonster)
                    SpawnMonsters(numberOfMonster - monsterCount);

                //detect end of game
                if (base1.CurrentHealth == 0 || base2.CurrentHealth == 0)
                {
                    if (base1.CurrentHealth == 0)
                        GameController.Singleton.winningTeam = 1;
                    else
                        GameController.Singleton.winningTeam = 0;
                    NetworkManager.Singleton.SceneManager.LoadScene("EndScene", LoadSceneMode.Single);
                    GetComponent<Gameloop>().enabled = false;
                }
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

                    Physics.IgnoreCollision(player.gameObject.GetComponent<Collider>(),
                        shield1.gameObject.GetComponent<MeshCollider>());
                }
                else
                {
                    foreach (Collider colliderchild in child)
                    {
                        Physics.IgnoreCollision(colliderchild, shield2.gameObject.GetComponent<MeshCollider>());
                    }

                    Physics.IgnoreCollision(player.gameObject.GetComponent<Collider>(),
                        shield2.gameObject.GetComponent<MeshCollider>());
                }
            }

            //create the timer
            var timer = currTime.Value - referenceTime.Value;
            HUDController.Singleton.SetTimer(MakeBeautyTimer(timer), timer.Minutes >= timeToEnd);

            if (IsServer)
            {
                if (timer.Minutes >= timeToEnd)
                {
                    shield1.Activated.Value = shield2.Activated.Value = false;
                    if(!deactivateAllCapturePoints)
                        DeactivateCapturePoints();
                    return;
                }
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
            //Activate all the shields
            shield1.Activated.Value = true;
            shield2.Activated.Value = true;
        }

        private void DeactivateCapturePoints()
        {
            foreach (GameObject area in captureArea)
            {
                area.GetComponent<ObjectiveController>().ToggleCanCapture(false);
            }

            deactivateAllCapturePoints = true;
        }

        private void SpawnMonsters(int number)
        {
            for (int i = 0; i < number; i++)
            {
                Vector3 pos = new Vector3(Random.Range(-285, 280), 60, Random.Range(-280, 275));
                Physics.Raycast(pos, Vector3.down, out RaycastHit hit);
                while (Vector3.Distance(hit.point, new Vector3(0,0,0)) < 150)
                {
                    pos = new Vector3(Random.Range(-285, 280), 60, Random.Range(-280, 275));
                    Physics.Raycast(pos, Vector3.down, out hit);
                }
                GameObject instance = Instantiate(GameController.Singleton.MonsterPrefab, hit.point,
                        Quaternion.identity);
                instance.GetComponent<NetworkObject>().Spawn();
                if (instance.GetComponent<NavMeshAgent>().isOnNavMesh)
                {
                    instance.GetComponent<MonsterController>().Life =
                        instance.GetComponent<MonsterController>().MaxHealth;
                    monsterCount++;
                }
                else
                {
                    instance.GetComponent<NetworkObject>().Despawn();
                }
            }
        }

        public void RemoveMonster(MonsterController monsterController)
        {
            monsterController.DestroyServerRpc();
            GameObject GrenadeInstance = Instantiate(GameController.Singleton.ItemPrefabs[1],
                monsterController.gameObject.transform.position, quaternion.identity);
            GrenadeInstance.GetComponent<NetworkObject>().Spawn();
        }

        IEnumerator Wait1Second()
        {
            yield return new WaitForSeconds(1);
            hasBeenChange = false;
        }

        private string MakeBeautyTimer(TimeSpan timeSpan)
        {
            string seconds = timeSpan.Seconds / 10 == 0 ? $"0{timeSpan.Seconds}" : $"{timeSpan.Seconds}";
            string minutes = timeSpan.Minutes / 10 == 0 ? $"0{timeSpan.Minutes}" : $"{timeSpan.Minutes}";
            string hours = timeSpan.Hours / 10 == 0 ? $"0{timeSpan.Hours}" : $"{timeSpan.Hours}";
            return $"{hours}:{minutes}:{seconds}";
        }
    }
}
