using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyMovement : MonoBehaviour
{
    public Transform player;
    public float updateSpeed = 0.1f; //Updating the path to player based on their position
    private NavMeshAgent agent;

    private Coroutine followCoroutine;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    public void StartChasing()
    {
        if(followCoroutine == null)
        {
            followCoroutine = StartCoroutine(MoveTowardPlayer());
        }
        else
        {
            Debug.LogWarning("Enemy is already chasing.");
        }
    }

    private IEnumerator MoveTowardPlayer()
    {
        WaitForSeconds Wait = new WaitForSeconds(updateSpeed);

        while (enabled)
        {
            agent.SetDestination(player.transform.position);

            yield return Wait;
        }
    }
}
