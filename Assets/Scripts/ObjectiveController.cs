using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using PlayerControllers;
using Unity.Netcode;
using UnityEngine;

public class ObjectiveController : NetworkBehaviour
{
    public enum State
    {
        Neutral,
        Capuring,
        Captured
    }
    
    /** <value>the material used for neutral point</value> */
    [SerializeField] private Material baseMaterial;
    
    /** <value>the material used for captured point</value> */
    [SerializeField] private Material firstTeamMaterial;
    [SerializeField] private Material secondTeamMaterial;

    /** <value>the GameObject used to trigger capture</value> */
    [SerializeField] private GameObject captureArea;

    private NetworkVariable<float> CurrentProgress { get; } = new NetworkVariable<float>(0f);
    
    public float CurrentProgressValue => CurrentProgress.Value;
    
    private int controllingTeam;
    private int capturingTeam;
    
    /** <value>the capture progress</value> */
    private float progress = 0f;
    
    /** <value>the progress needed to capture a point</value> */
    private float maxProgress = 5;
    
    /** <value>the progress speed</value> */
    private float progressSpeed = 1f;
    
    /** <value>the state of an objective, whether Neutral or Captured</value> */
    private State state;
    
    /** <value>the list of all players on an objective</value> */
    private List<BasePlayer> capturingPlayersList = new List<BasePlayer>();

    public float Progress => progress;
    public float MaxProgress => maxProgress;

    public List<BasePlayer> CapturingPlayersList => capturingPlayersList;
    
    public static event Action<ObjectiveController,BasePlayer,bool> OnPlayerInteract;
    // TODO: public static event Action<bool> OnCaptured;
    
    // Start is called before the first frame update
    void Start()
    {
        state = State.Neutral;
    }

    // Update is called once per frame
    void Update()
    {
        if (state == State.Neutral)
        {
            if (capturingPlayersList.Count != 0)
            {
                capturingTeam = CapturingPlayersList[0].TeamId;
                controllingTeam = CapturingPlayersList[0].TeamId;
                state = State.Capuring;
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

            Debug.Log($"Team 1 has {t1PlayerList.Count + 0} players");
            //Debug.Log($"Team 2 has {t2PlayerList.Count + 0} players");
            
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

            Debug.Log(capturingTeam);
            Debug.Log(controllingTeam);
            if (capturingTeam == 0 && controllingTeam == 1 || capturingTeam == 1 && controllingTeam == 0)
            {
                CurrentProgress.Value -= nbrPlayersCapturing * progressSpeed * Time.deltaTime;
                Debug.Log("downcap");
            }
            else if (controllingTeam == capturingTeam)
            {
                CurrentProgress.Value += nbrPlayersCapturing * progressSpeed * Time.deltaTime;
                Debug.Log("upcap");
            }
            Debug.Log(CurrentProgress.Value);

            if (CurrentProgress.Value < 0)
            {
                CurrentProgress.Value = 0;
                state = State.Neutral;
            }
            else if (CurrentProgress.Value > maxProgress)
            {
                state = State.Captured;
                Debug.Log($"Point captured by team {controllingTeam}");
            }
            
            //Debug.Log(CurrentProgress.Value);
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
}
