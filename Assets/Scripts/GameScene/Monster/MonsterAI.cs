using System.Collections;
using GameScene.PlayerControllers.BasePlayer;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

namespace GameScene.Monster
{
    public class MonsterAI : NetworkBehaviour
    {
        private float detectDistance = 35; //distance de detection du joueur
        private float distanceAttack = 5f; //distance à laquelle le monstre peut attaquer
        private Vector3 InitialPos; //position d'origine du monstre
        private GameObject joueur; //référence vers le(s) joueur(s)
        private bool canAttack = true; //le monstre peut attaquer ou non
        public NavMeshAgent agent;
        private float dist;
        private float mindist = 1000f;


        private void Start()
        {
            InitialPos = transform.position; //initialisation de la position d'origine
            agent = GetComponent<NavMeshAgent>();
        }


        private void Update()
        {
            if(!agent.isOnNavMesh)
                return;
        
            if (gameObject != null)
            {
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

        IEnumerator Wait()
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
}