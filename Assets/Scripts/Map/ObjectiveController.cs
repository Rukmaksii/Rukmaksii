using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using PlayerControllers;
using Unity.Netcode;
using UnityEngine;

namespace Map
{
        public class ObjectiveController : MonoBehaviour
    {
        public enum State
        {
            Neutral,
            Capuring,
            Captured
        }
                
        /** <value>the material used for captured point</value> */
        [SerializeField] private Material firstTeamMaterial;
        [SerializeField] private Material secondTeamMaterial;
        /** <value>the material used for neutral point</value> */
        [SerializeField] private Material neutralMaterial;

        /** <value>the GameObject used to trigger capture</value> */
        [SerializeField] private GameObject captureArea;

        private float currentProgress = 0f;

        /** <value>the capture progress</value> */
        public float CurrentProgress => currentProgress;
        
        /** <value>teamID of the team whose players are more numerous on the point</value> */
        private int controllingTeam;
        /** <value>teamID of the team whose progress is registered</value> */
        private int capturingTeam;

        public int ControllingTeam => controllingTeam;
        public int CapturingTeam => capturingTeam;

       

        /** <value>the progress needed to capture a point</value> */
        private float maxProgress = 5;
        
        /** <value>the progress speed</value> */
        private float progressSpeed = 1f;
        
        /** <value>the state of an objective, whether Neutral or Captured</value> */
        private State state;
        
        /** <value>the list of all players on an objective</value> */
        private List<BasePlayer> capturingPlayersList = new List<BasePlayer>();

        public float MaxProgress => maxProgress;

        public List<BasePlayer> CapturingPlayersList => capturingPlayersList;
        
        public static event Action<ObjectiveController,BasePlayer,bool> OnPlayerInteract;

        /** <value>boolean that indicates whether the objectives can be captured or not*/
        public bool canCapture = false;

        public bool CanCapture => canCapture;
        // TODO: public static event Action<bool> OnCaptured;
        
        // Start is called before the first frame update
        void Start()
        {
            this.state = State.Neutral;
        }

        // Update is called once per frame
        void Update()
        {
            if (state == State.Neutral)
            {
                captureArea.GetComponent<MeshRenderer>().material = neutralMaterial;
                currentProgress = 0;
                capturingTeam = -1;
                controllingTeam = -1;
                if (!canCapture)
                    return;
                if (capturingPlayersList.Count != 0)
                {
                    capturingTeam = CapturingPlayersList[0].TeamId;
                    controllingTeam = CapturingPlayersList[0].TeamId;
                    this.state = State.Capuring;
                }
            }
            
            else if (state == State.Capuring)
            {
                (List<BasePlayer> t1PlayerList, List<BasePlayer> t2PlayerList) = (new List<BasePlayer>(), new List<BasePlayer>(0));

                foreach (BasePlayer player in CapturingPlayersList)
                {
                    if (player.TeamId == 0)
                        t1PlayerList.Add(player);
                    else if (player.TeamId == 1)
                        t2PlayerList.Add(player);
                }

                int nbrPlayersCapturing;

                if (t1PlayerList.Count > t2PlayerList.Count)
                {
                    controllingTeam = 0;
                    nbrPlayersCapturing = t1PlayerList.Count;
                }
                else if (t1PlayerList.Count < t2PlayerList.Count)
                {
                    controllingTeam = 1;
                    nbrPlayersCapturing = t2PlayerList.Count;
                }
                else
                {
                    controllingTeam = -1;
                    nbrPlayersCapturing = 0;
                }
                
                if (capturingTeam == 0 && controllingTeam == 1 || capturingTeam == 1 && controllingTeam == 0)
                {
                    currentProgress -= nbrPlayersCapturing * progressSpeed * Time.deltaTime;
                }
                else if (controllingTeam == capturingTeam)
                { 
                    currentProgress += nbrPlayersCapturing * progressSpeed * Time.deltaTime;
                }

                if (CurrentProgress < 0)
                {
                    state = State.Neutral;
                }
                else if (CurrentProgress > maxProgress)
                {
                    state = State.Captured;
                }
            }
            
            else if (state == State.Captured)
            {
                if (capturingTeam == 0)
                    captureArea.GetComponent<MeshRenderer>().material = firstTeamMaterial;
                else if (capturingTeam == 1)
                    captureArea.GetComponent<MeshRenderer>().material = secondTeamMaterial;
            }
        }

        private void OnTriggerEnter(Collider collider)
        {
            if (collider.CompareTag("Player"))
            {
                OnPlayerInteract?.Invoke(this,collider.GetComponent<BasePlayer>(),true);
                capturingPlayersList.Add(collider.GetComponent<BasePlayer>());

            }
        }
        
        private void OnTriggerExit(Collider collider)
        {
            if (collider.CompareTag("Player"))
            {
                OnPlayerInteract?.Invoke(this,collider.GetComponent<BasePlayer>(),false);
                capturingPlayersList.Remove(collider.GetComponent<BasePlayer>());
            }
        }

        public void ToggleCanCapture(bool status)
        {
            canCapture = status;
            state = State.Neutral;
        }
    }
}