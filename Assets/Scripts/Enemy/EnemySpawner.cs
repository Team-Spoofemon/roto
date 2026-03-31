using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemySpawner : MonoBehaviour
{
    private Transform player;
    private Collider playerCollider;
    private Collider spawnZoneCollider;

    [SerializeField] private float minSpawnSpacing = 2f;
    [SerializeField] private int maxSpawnAttemptsPerEnemy = 20;

    public Collider barrier;
    public float EnemyPerTile = 0.5f;
    public int numberOfEnemies = 5;
    public float spawnDelay = 1f;
    public List<Enemy> EnemyPrefabs = new List<Enemy>();
    public SpawnMethod enemySpawnMethod = SpawnMethod.EarthGiant;

    private Dictionary<int, ObjectPool> EnemyObjectPools = new Dictionary<int, ObjectPool>();
    private bool hasSpawned;

    public enum SpawnMethod
    {
        EarthGiant,
        Random
    }

    private void Awake()
    {
        spawnZoneCollider = GetComponent<Collider>();

        for (int i = 0; i < EnemyPrefabs.Count; i++)
        {
            EnemyObjectPools.Add(i, ObjectPool.CreateInstance(EnemyPrefabs[i], numberOfEnemies));
        }
    }

    private void Start()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

        if (playerObject != null)
        {
            player = playerObject.transform;
            playerCollider = playerObject.GetComponentInChildren<Collider>();
            Debug.Log(name + " found player: " + player.name);
        }
        else
        {
            Debug.LogError(name + " could not find player with tag 'Player'.");
        }

        if (spawnZoneCollider == null)
        {
            Debug.LogError(name + " has no collider on the spawn zone object.");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (hasSpawned)
            return;

        if (!other.CompareTag("Player"))
            return;

        Debug.Log(name + " OnTriggerExit fired by: " + other.name);
        hasSpawned = true;
        StartCoroutine(SpawnEnemies());
    }

    private IEnumerator SpawnEnemies()
    {
        Debug.Log(name + " SpawnEnemies started");

        WaitForSeconds wait = new WaitForSeconds(spawnDelay);
        List<Vector3> usedSpawnPositions = new List<Vector3>();

        int spawnedEnemies = 0;

        while (spawnedEnemies < numberOfEnemies)
        {
            bool spawned = false;

            if (enemySpawnMethod == SpawnMethod.EarthGiant)
            {
                spawned = SpawnEarthGiantEnemy(spawnedEnemies, usedSpawnPositions);
            }
            else if (enemySpawnMethod == SpawnMethod.Random)
            {
                spawned = SpawnRandomEnemy(usedSpawnPositions);
            }

            if (spawned)
            {
                spawnedEnemies++;
                yield return wait;
            }
            else
            {
                Debug.LogWarning(name + " failed to find a valid spawn position.");
                break;
            }
        }
    }

    private bool SpawnEarthGiantEnemy(int spawnedEnemies, List<Vector3> usedSpawnPositions)
    {
        int spawnIndex = spawnedEnemies % EnemyPrefabs.Count;
        return DoSpawnEnemy(spawnIndex, usedSpawnPositions);
    }

    private bool SpawnRandomEnemy(List<Vector3> usedSpawnPositions)
    {
        return DoSpawnEnemy(Random.Range(0, EnemyPrefabs.Count), usedSpawnPositions);
    }

    private bool DoSpawnEnemy(int spawnIndex, List<Vector3> usedSpawnPositions)
    {
        if (spawnZoneCollider == null)
        {
            Debug.LogError(name + " has no spawn zone collider.");
            return false;
        }

        PoolableObject poolableObject = EnemyObjectPools[spawnIndex].GetObject();

        if (poolableObject == null)
        {
            Debug.LogError(name + " could not get enemy from pool at index: " + spawnIndex);
            return false;
        }

        Enemy enemy = poolableObject.GetComponent<Enemy>();

        if (enemy == null)
        {
            Debug.LogError(name + " pooled object does not have an Enemy component.");
            poolableObject.gameObject.SetActive(false);
            return false;
        }

        if (enemy.agent == null)
        {
            Debug.LogError(name + " enemy has no NavMeshAgent.");
            poolableObject.gameObject.SetActive(false);
            return false;
        }

        enemy.agent.enabled = false;

        Vector3 spawnPosition;
        if (!TryGetSpawnPosition(usedSpawnPositions, out spawnPosition))
        {
            poolableObject.gameObject.SetActive(false);
            return false;
        }

        enemy.transform.position = spawnPosition;

        enemy.agent.enabled = true;

        if (!enemy.agent.Warp(spawnPosition))
        {
            Debug.LogWarning(name + " failed to warp enemy to: " + spawnPosition);
            enemy.agent.enabled = false;
            poolableObject.gameObject.SetActive(false);
            return false;
        }

        enemy.SetTarget(player, playerCollider);
        enemy.movement.StartChasing();

        usedSpawnPositions.Add(spawnPosition);

        Debug.Log(name + " enemy spawned successfully at: " + spawnPosition);
        return true;
    }

    private bool TryGetSpawnPosition(List<Vector3> usedSpawnPositions, out Vector3 validPosition)
    {
        Bounds bounds = spawnZoneCollider.bounds;

        for (int attempt = 0; attempt < maxSpawnAttemptsPerEnemy; attempt++)
        {
            Vector3 randomPoint = new Vector3(
                Random.Range(bounds.min.x, bounds.max.x),
                bounds.center.y,
                Random.Range(bounds.min.z, bounds.max.z)
            );

            if (!IsPointInsideSpawnZone(randomPoint))
                continue;

            NavMeshHit hit;
            if (!NavMesh.SamplePosition(randomPoint, out hit, 3f, NavMesh.AllAreas))
                continue;

            if (IsTooCloseToOtherSpawns(hit.position, usedSpawnPositions))
                continue;

            validPosition = hit.position;
            return true;
        }

        validPosition = Vector3.zero;
        return false;
    }

    private bool IsPointInsideSpawnZone(Vector3 point)
    {
        Vector3 closestPoint = spawnZoneCollider.ClosestPoint(point);
        return Vector3.Distance(closestPoint, point) < 0.05f;
    }

    private bool IsTooCloseToOtherSpawns(Vector3 position, List<Vector3> usedSpawnPositions)
    {
        for (int i = 0; i < usedSpawnPositions.Count; i++)
        {
            if (Vector3.Distance(position, usedSpawnPositions[i]) < minSpawnSpacing)
                return true;
        }

        return false;
    }
}