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
        Captured
    }
    
    /** <value>the material used for neutral point</value> */
    [SerializeField] private Material baseMaterial;
    
    /** <value>the material used for captured point</value> */
    [SerializeField] private Material capturedMaterial;
    
    /** <value>the GameObject used to trigger capture</value> */
    [SerializeField] private GameObject captureArea;
    
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
    public static event Action<bool> OnCaptured;
    
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
            int playersCapturing = CapturingPlayersList.Count;
        
            progress += playersCapturing * progressSpeed * Time.deltaTime;
            
            if (progress > maxProgress)
            {
                state = State.Captured;
                Debug.Log("Point captured!");
            }
        }
        else
        {
            captureArea.GetComponent<MeshRenderer>().material = capturedMaterial;
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
