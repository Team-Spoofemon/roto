using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyMovement : MonoBehaviour
{
    [SerializeField] private Animator enemyAnim;
    [SerializeField] private SpriteOrientation spriteOrientation;
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
        animator = GetComponentInChildren<Animator>();

        if (spriteOrientation == null)
            spriteOrientation = GetComponentInChildren<SpriteOrientation>();
    }

    private void Update()
    {
        enemyAnim.SetBool("isPatroling", agent.velocity.magnitude > 0.01f);

        if (spriteOrientation != null && agent.velocity.sqrMagnitude > 0.01f && Camera.main != null)
        {
            Vector3 camRight = Camera.main.transform.right;
            camRight.y = 0f;
            camRight.Normalize();

            float horizontal = Vector3.Dot(agent.velocity, camRight);

            if (Mathf.Abs(horizontal) > 0.08f)
                spriteOrientation.SetFacingRight(horizontal > 0f);
        }
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
        Debug.Log("SetAttacking: " + value);
        isAttacking = value;
        animator.SetBool("isAttacking", value);

        if (value)
        {
            agent.isStopped = true;
            animator.SetBool("isPatroling", false);
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
                animator.SetBool("isPatroling", isMoving);

                if (spriteOrientation != null && agent.velocity.sqrMagnitude > 0.01f && Camera.main != null)
                {
                    Vector3 camRight = Camera.main.transform.right;
                    camRight.y = 0f;
                    camRight.Normalize();

                    float horizontal = Vector3.Dot(agent.velocity, camRight);

                    if (Mathf.Abs(horizontal) > 0.08f)
                        spriteOrientation.SetFacingRight(horizontal > 0f);
                }
            }

            yield return Wait;
        }
    }
} 