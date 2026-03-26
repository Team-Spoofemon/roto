using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyMovement : MonoBehaviour
{
    [SerializeField] private Animator enemyAnim;
    public Transform player;
    public float updateSpeed = 0.1f; //Updating the path to player based on their position
    private NavMeshAgent agent;
    private string IsWalking = "isPatroling";
    private string MeleeHit = "isMeleeHit";

    private Coroutine followCoroutine;

    private Animator animator;
    private bool isAttacking;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        enemyAnim.SetBool("isPatroling", agent.velocity.magnitude > 0.01f);
    }

    //If not currently chasing, begin the process for following the player
    public void StartChasing()
    {
        if(followCoroutine == null)
        {
            followCoroutine = StartCoroutine(MoveTowardPlayer());
            enemyAnim.SetBool("isPatroling", agent.velocity.magnitude > 0.01f);
        }
        else
        {
            Debug.LogWarning("Enemy is already chasing.");
        }
    }

    public void SetAttacking(bool value)
    {
        isAttacking = value;
        animator.SetBool("isAttacking", value);

        if (value)
        {
            agent.isStopped = true;
            animator.SetBool("isWalking", false);
        }
        else
        {
            agent.isStopped = false;
        }
    }

    //Starts process of following the player
    private IEnumerator MoveTowardPlayer()
    {
        WaitForSeconds Wait = new WaitForSeconds(updateSpeed);

        while (enabled)
        {
            if (!isAttacking)
            {
                //NavMeshAgent sets destination to the player's position
                agent.SetDestination(player.transform.position);

                bool isMoving = agent.velocity.magnitude > 0.1f;
                animator.SetBool("isWalking", isMoving);
            }

            yield return Wait;
        }
    }
}