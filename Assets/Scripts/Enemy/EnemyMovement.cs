using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyMovement : MonoBehaviour
{
    [SerializeField] private Animator enemyAnim;
    [SerializeField] private Animator rootsAnim;
    [SerializeField] private SpriteOrientation spriteOrientation;
    public Transform player;
    public float updateSpeed = 0.1f;

    private NavMeshAgent agent;
    private const string IsWalking = "isPatroling";
    private Coroutine followCoroutine;
    private bool isAttacking;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();

        if (enemyAnim == null)
            enemyAnim = GetComponentInChildren<Animator>();

        if (spriteOrientation == null)
            spriteOrientation = GetComponentInChildren<SpriteOrientation>();

        if (player == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null)
                player = playerObject.transform;
        }
    }

    private void Update()
    {
        if (enemyAnim == null || agent == null)
            return;

        if (isAttacking)
        {
            enemyAnim.SetBool(IsWalking, false);
            return;
        }

        if (!agent.enabled || !agent.isOnNavMesh)
        {
            enemyAnim.SetBool(IsWalking, false);
            return;
        }

        enemyAnim.SetBool(IsWalking, agent.velocity.magnitude > 0.01f);

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

    public void StartChasing()
    {
        if (followCoroutine == null)
            followCoroutine = StartCoroutine(MoveTowardPlayer());
    }

    public void SetAttacking(bool value)
    {
        Debug.Log("SetAttacking: " + value);
        isAttacking = value;

        if (agent == null || !agent.enabled || !agent.isOnNavMesh)
            return;

        if (value)
        {
            agent.isStopped = true;
            agent.ResetPath();

            if (enemyAnim != null)
                enemyAnim.SetBool(IsWalking, false);
        }
        else
        {
            agent.isStopped = false;
        }
    }

    private IEnumerator MoveTowardPlayer()
    {
        WaitForSeconds wait = new WaitForSeconds(updateSpeed);

        while (enabled)
        {
            if (
                !isAttacking &&
                player != null &&
                agent != null &&
                agent.enabled &&
                agent.isOnNavMesh
            )
            {
                agent.SetDestination(player.position);
            }

            yield return wait;
        }

        followCoroutine = null;
    }
}