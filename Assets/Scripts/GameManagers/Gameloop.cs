using Unity.Netcode;
using UnityEngine;
using System;
using System.Collections;
using Map;
using Random = UnityEngine.Random;

namespace GameManagers
{
    public class Gameloop : NetworkBehaviour
    {
        private NetworkVariable<DateTime> referenceTime = new NetworkVariable<DateTime>();
        private NetworkVariable<DateTime> currTime = new NetworkVariable<DateTime>();

        private int objectiveDelay;
        private bool hasBeenChange;
        

        // Start is called before the first frame update
        void Start()
        {
            if (NetworkManager.Singleton.IsServer)
            {
                referenceTime.Value = DateTime.Now;
                SpawnMonsters();
            }

            objectiveDelay = 5;
            hasBeenChange = false;
        }

        // Update is called once per frame
        void Update()
        {
            if (NetworkManager.Singleton.IsServer)
                currTime.Value = DateTime.Now;
            var timer = currTime.Value - referenceTime.Value;
            if (timer.Minutes % objectiveDelay == 0 && timer.Seconds == 0 && !hasBeenChange)
            {
                ChangeCapturePoints();
                hasBeenChange = true;
                StartCoroutine(Wait1Second());
            }
        }


        public void ChangeCapturePoints()
        {
            var captureArea = GameObject.FindGameObjectsWithTag("CaptureArea");
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

        public void SpawnMonsters()
        {
            for (int i = 0; i < 4; i++)
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
