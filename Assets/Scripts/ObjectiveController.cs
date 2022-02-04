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
    
    [SerializeField] private Material baseMaterial;
    [SerializeField] private Material capturedMaterial;
    [SerializeField] private GameObject captureArea;
    
    private float progress = 0f;
    private float progressSpeed = 1f;
    private State state;
    
    private List<BasePlayer> capturingPlayersList = new List<BasePlayer>();

    public float Progress => progress;

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
            
            if (progress > 4)
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
