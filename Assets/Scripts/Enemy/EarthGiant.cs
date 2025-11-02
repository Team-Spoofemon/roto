using UnityEngine;
using UnityEngine.AI;

public class EarthGiant : MonoBehaviour, IHitHandler
{
    public NavMeshAgent agent;
    public Transform player;

    [SerializeField]
    private Rigidbody rb;

    public float attackDistance = 3f;
    private float distance;

    private Vector3 origin;
    private bool sights = true;

    [SerializeField]
    private float damage;

    [SerializeField]
    private float damageKnockback;

    private void Update()
    {
        MoveTowardPlayer();
    }

    private void MoveTowardPlayer()
    {

        if (player == null)
        {
            //Try to find the new player after respawn
            GameObject newPlayer = GameObject.FindGameObjectWithTag("Player");
            if (newPlayer != null)
                player = newPlayer.transform;
            else
                return;
            
        }

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

    public void OnHit(HealthManager targetHealth)
    {
        CombatManager.Instance.SingleAttack(targetHealth, damage, transform, damageKnockback);
    }
}
