using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using PlayerControllers;
using UnityEngine;

public class ObjectiveController : MonoBehaviour
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

            Debug.Log($"Team 1 has {t1PlayerList.Capacity + 0} players");
            Debug.Log($"Team 2 has {t2PlayerList.Capacity + 0} players");
            
            controllingTeam = t1PlayerList.Count > t2PlayerList.Count ? 0 : 1;

            int nbrPlayersCapturing;

            if (capturingTeam == 0)
                nbrPlayersCapturing = t1PlayerList.Count;
            else if (capturingTeam == 1)
                nbrPlayersCapturing = t2PlayerList.Count;
            else
                nbrPlayersCapturing = 0;
            
            if (capturingTeam != controllingTeam)
                progress -= nbrPlayersCapturing * progressSpeed * Time.deltaTime;
            else
                progress += nbrPlayersCapturing * progressSpeed * Time.deltaTime;

            if (progress < 0)
            {
                progress = 0;
                state = State.Neutral;
                capturingTeam = -1;
            }
            else if (progress > maxProgress)
            {
                state = State.Captured;
                Debug.Log($"Point captured by team {controllingTeam}");
            }
            
            Debug.Log(progress);
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
        Debug.Log("something entered");
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
