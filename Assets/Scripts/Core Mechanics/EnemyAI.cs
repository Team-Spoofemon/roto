using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    public NavMeshAgent agent;
    public Transform player;
    public float attackDistance;
    private float distance;
    private Vector3 origin;
    private bool sights = true;

    void Start()
    {
        //Check for agent component on enemy at the start
        agent = GetComponent<NavMeshAgent>();
        //Stores the origin of the enemy
        origin = transform.position;
    }

    void Update()
    {
        //Stores the current distance between the enemy and the player
        distance = Vector2.Distance(agent.transform.position, player.position);

        if (distance < attackDistance)
        {
            //If player is in range of enemy attack, enemy will stop moving
            agent.isStopped = true;
        }
        else
        {
            //Set enemy as moving
            agent.isStopped = false;

            if (!agent.hasPath && sights)
            {
                //If enemy is off path and has sights on player, then enemy will move back to original point and sights set to false
                agent.SetDestination(origin);
                sights = false;
            }
            else
            {
                //If player is far from range of enemy attack, enemy moves to the player's position
                agent.SetDestination(player.position);
                sights = true;
            }
        }
    }

    
}
