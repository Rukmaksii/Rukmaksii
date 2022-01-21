using System;
using System.Collections;
using Unity.Netcode;
using Unity.Netcode.Samples;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Interactions;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

public class MonsterAI : MonoBehaviour
{
    [Range(2, 100)] public float detectDistance = 10; //distance de detection du joueur
    public float distanceAttack = 2.4f; //distance à laquelle le monstre peut attaquer
    private Vector3 InitialPos; //position d'origine du monstre
    //public GameObject col; //collider servant de machoir pour le monstre (ce qui va faire des dégats)
    private Transform joueur; //référence vers le(s) joueur(s)
    private bool canAttack = true; //le monstre peut attaquer ou non
    public NavMeshAgent agent;
    private float dist;
    private float mindist = 1000f;


    private void Start()
    {
        InitialPos = transform.position; //initialisation de la position d'origine
    }

    

    private void Update()
    {
        if (gameObject != null)
        {
            foreach (Transform players in GameObject.FindGameObjectWithTag("Player").transform)
            {
                dist = Vector3.Distance(transform.position, players.position);
                if (dist < mindist)
                {
                    mindist = dist;
                    joueur = players;
                }
            }
        }
        float distance = Vector3.Distance(transform.position, joueur.position); //distance entre le monstre et le joueur
        if (distance < detectDistance && distance > distanceAttack) //le joueur est visible mais pas à distance d'attaque
        {
            //le monstre pourchasse le joueur
            agent.destination = joueur.position;
        }
        

        if (distance <= distanceAttack && canAttack) //le joueur est à distance d'attaque
        {
            //le monstre attaque le joueur
        }

        if (distance > detectDistance) //le joueur n'est plus visible
        {
            //le monstre retourne à son point de départ ou rode dans les parages
            agent.destination = InitialPos;
        }
    }


    private void OnDrawGizmosSelected() // permet de voir la sphere du champ de detection du monstre
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, detectDistance);
    }
}
