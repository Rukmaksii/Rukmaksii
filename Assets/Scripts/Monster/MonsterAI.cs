using System;
using System.Collections;
using PlayerControllers;
using Unity.Netcode;
using Unity.Netcode.Samples;
//using UnityEditor.AssetImporters;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Interactions;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

public class MonsterAI : NetworkBehaviour
{
    [Range(2, 100)] public float detectDistance = 10; //distance de detection du joueur
    public float distanceAttack = 2.4f; //distance à laquelle le monstre peut attaquer
    private Vector3 InitialPos; //position d'origine du monstre
    //public GameObject col; //collider servant de machoir pour le monstre (ce qui va faire des dégats)
    private GameObject joueur; //référence vers le(s) joueur(s)
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
            //Debug.Log("recherche du joueur");
            foreach (var players in GameObject.FindGameObjectsWithTag("Player"))
            {
                dist = Vector3.Distance(transform.position, players.transform.position);
                if (dist < mindist)
                {
                    mindist = dist;
                    joueur = players;
                }
            }
        }

        if (joueur != null)
        {
            //Debug.Log("Joueur trouvé!!!");
            float distance = Vector3.Distance(transform.position, joueur.transform.position); //distance entre le monstre et le joueur
            if (distance < detectDistance && distance > distanceAttack) //le joueur est visible mais pas à distance d'attaque
            {
                NavMeshPath path = new NavMeshPath();
                agent.CalculatePath(joueur.transform.position, path);
                //le monstre pourchasse le joueur
                if (path.status == NavMeshPathStatus.PathComplete)
                {
                    agent.SetPath(path);
                }
                //Debug.Log($"position du joueur {joueur.position} destination {agent.destination} initial pos {InitialPos}");
            }
        

            if (distance <= distanceAttack && canAttack) //le joueur est à distance d'attaque
            {
                //le monstre attaque le joueur
                canAttack = false;
                joueur.GetComponent<BasePlayer>().TakeDamage(5);
                StartCoroutine(Wait());
            }

            if (distance > detectDistance) //le joueur n'est plus visible
            {
                //le monstre retourne à son point de départ ou rode dans les parages
                agent.destination = InitialPos;
            }
        }
    }
    private IEnumerator Wait()
    {
        yield return new WaitForSeconds(1);
        canAttack = true;
    }

   
    private void OnDrawGizmosSelected() // permet de voir la sphere du champ de detection du monstre
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, detectDistance);
    }
}
