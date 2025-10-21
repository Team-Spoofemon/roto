using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EarthGiantOLD : Entity
{
    public NavMeshAgent agent;
    public Transform player;
    public float attackDistance = 3f;
    private float distance;
    private Vector3 origin;
    private bool sights = true;

    [Header("Knockback")]
    [SerializeField]
    float knockbackForce = 30f;

    [SerializeField]
    float knockbackUpward = 0.2f;

    [SerializeField]
    float knockbackDuration = 0.12f;

    [SerializeField]
    float knockbackSpeed = 6f;

    [SerializeField]
    float fallbackNudge = 0.35f;

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

    private void OnCollisionEnter(Collision collision)
    {
        GameObject other = collision.gameObject;

        // Prefer the base type your Attack expects
        var damageable = other.GetComponent<IDamageable>();
        if (damageable != null)
        {
            damageable.TakeDamage(attackDamage);
            // ApplyKnockback(entity.transform); // if you re-enable later
            return;
        }
    }

    public override void Die()
    {
        Destroy(gameObject);
    }

    // void ApplyKnockback(Transform target)
    // {
    //     // horizontal push away from the giant
    //     Vector3 dir = (target.position - transform.position);
    //     dir.y = 0f;
    //     if (dir.sqrMagnitude < 0.0001f) dir = transform.forward; // just in case
    //     dir.Normalize();

    //     // Prefer Rigidbody impulse
    //     var rb = target.GetComponent<Rigidbody>();
    //     if (rb != null && !rb.isKinematic)
    //     {
    //         rb.AddForce(dir * knockbackForce + Vector3.up * knockbackUpward, ForceMode.Impulse);
    //         return;
    //     }

    //     var cc = target.GetComponent<CharacterController>();
    //     if (cc != null)
    //     {
    //         StartCoroutine(KnockbackCC(cc, dir));
    //         return;
    //     }
    //     target.position += dir * fallbackNudge;
    // }

    // IEnumerator KnockbackCC(CharacterController cc, Vector3 dir)
    // {
    //     float t = 0f;
    //     while (t < knockbackDuration)
    //     {
    //         cc.Move((dir * knockbackSpeed) * Time.deltaTime);
    //         t += Time.deltaTime;
    //         yield return null;
    //     }
    // }
}
