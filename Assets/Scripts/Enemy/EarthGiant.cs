using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.AI;

public class EarthGiant : MonoBehaviour
{
    public EntityStats _baseStats;
    public NavMeshAgent agent;
    public Transform player;
    public float attackDistance = 3f;
    private float distance;
    private Vector3 origin;
    private bool sights = true;

    private void Start()
    {
        _baseStats = GetComponent<EntityStats>();
        if (_baseStats == null)
            Debug.LogError("EarthGiant is missing EntityStats on the same GameObject.");
        else
            Debug.Log(gameObject.name + " damage stats acquired!");
    }

    private void Update()
    {
        MoveTowardPlayer();
    }

    private void MoveTowardPlayer()
    {
        // //Stores the current distance between the enemy and the player
        // distance = Vector2.Distance(agent.transform.position, player.position);

        // if (distance < attackDistance)
        // {
        //     //If player is in range of enemy attack, enemy will stop moving
        //     agent.isStopped = true;
        // }
        // else
        // {
        //     //Set enemy as moving
        //     agent.isStopped = false;

        //     if (!agent.hasPath && sights)
        //     {
        //         //If enemy is off path and has sights on player, then enemy will move back to original point and sights set to false
        //         agent.SetDestination(origin);
        //         sights = false;
        //     }
        //     else
        //     {
        //         //If player is far from range of enemy attack, enemy moves to the player's position
        //         agent.SetDestination(player.position);
        //         sights = true;
        //     }
        // }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            Debug.Log("Hit: " + other.gameObject.name + "!");
            DamageManager target = other.GetComponent<DamageManager>();
            Debug.Log("(" + other.gameObject.name + ") DM Status: " + target.GetType().Name);
            CombatManager.Instance.SingleAttack(target, _baseStats.baseDamage);
        }
    }
}
