using System;
using Unity.Netcode;
using Unity.Netcode.Samples;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Interactions;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

public class monsterAI : MonoBehaviour
{
    [Range(2, 100)] public float detectDistance = 10; //distance de detection du joueur


    private void OnDrawGizmosSelected() // permet de voir la sphere du champ de detection du monstre
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, detectDistance);
    }
}
