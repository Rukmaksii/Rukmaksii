using Unity.Netcode;
using UnityEngine;
using System;
using Random = UnityEngine.Random;

namespace GameManagers
{


    public class Gameloop : NetworkBehaviour
    {
        private NetworkVariable<DateTime> referenceTime = new NetworkVariable<DateTime>();
        private NetworkVariable<DateTime> currTime = new NetworkVariable<DateTime>();


        // Start is called before the first frame update
        void Start()
        {
            if (NetworkManager.Singleton.IsServer)
            {
                referenceTime.Value = DateTime.Now;
                SpawnMonsters();
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (NetworkManager.Singleton.IsServer)
                currTime.Value = DateTime.Now;
            TimeSpan timer = currTime.Value - referenceTime.Value;
            Debug.Log($"{timer.Hours}:{timer.Minutes}:{timer.Seconds}");
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
                    GameObject instance = Instantiate(GameController.Singleton.MonsterPrefab, hit.point,
                        Quaternion.identity);
                    instance.GetComponent<NetworkObject>().Spawn();
                }
            }
        }
    }
}
