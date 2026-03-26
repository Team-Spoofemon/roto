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

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    private void Update()
    {
        //enemyAnim.SetBool("isPatroling", agent.velocity.magnitude > 0.01f);
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

    //Starts process of following the player
    private IEnumerator MoveTowardPlayer()
    {
        WaitForSeconds Wait = new WaitForSeconds(updateSpeed);

        while (enabled)
        {
            //NavMeshAgent sets destination to the player's position
            agent.SetDestination(player.transform.position);

            yield return Wait;
        }
    }
}
