using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public Transform Player;
    private GameObject newPlayer;
    public float EnemyPerTile =0.5f;
    public int numberOfEnemies = 5;
    public float spawnDelay = 1f;
    public List<Enemy> EnemyPrefabs = new List<Enemy>();
    public SpawnMethod enemySpawnMethod = SpawnMethod.EarthGiant;

    public UnityEngine.AI.NavMeshTriangulation Triangulation;
    private Dictionary<int, ObjectPool> EnemyObjectPools = new Dictionary<int, ObjectPool>();

    public enum SpawnMethod
    {
        EarthGiant,
        Random
    }

    private void Awake()
    {
        for (int i = 0; i < EnemyPrefabs.Count; i++)
        {
            EnemyObjectPools.Add(i, ObjectPool.CreateInstance(EnemyPrefabs[i], numberOfEnemies));
        }
    }

    private void Start()
    {
        newPlayer = GameObject.FindGameObjectWithTag("Player");
        Triangulation = UnityEngine.AI.NavMesh.CalculateTriangulation();
    }

    private void OnTriggerExit(Collider collider)
    {
        StartCoroutine(SpawnEnemies());
    }

    private IEnumerator SpawnEnemies()
    {
        //Creates delay for spawning enemies
        WaitForSeconds Wait = new WaitForSeconds(spawnDelay);

        int spawnedEnemies = 0;

        while (spawnedEnemies < numberOfEnemies)
        {
            if(enemySpawnMethod == SpawnMethod.EarthGiant)
            {
                SpawnEarthGiantEnemy(spawnedEnemies);
            }
            else if(enemySpawnMethod == SpawnMethod.Random)
            {
                SpawnRandomEnemy();
            }

            spawnedEnemies++; //spawns one chosen enemy type

            yield return Wait; //activates delay after spawning in the enemy
        }
    }

    private void SpawnEarthGiantEnemy(int spawnedEnemies)
    {
        int SpawnIndex = spawnedEnemies % EnemyPrefabs.Count;

        DoSpawnEnemy(SpawnIndex);
    }

    private void SpawnRandomEnemy()
    {
        DoSpawnEnemy(Random.Range(0, EnemyPrefabs.Count));
    }

    private void DoSpawnEnemy(int SpawnIndex)
    {
        PoolableObject poolableObject = EnemyObjectPools[SpawnIndex].GetObject();

        if(poolableObject != null)
        {
            Enemy enemy = poolableObject.GetComponent<Enemy>();

            int vertexIndex = Random.Range(0, Triangulation.vertices.Length);

            UnityEngine.AI.NavMeshHit Hit;
            if(UnityEngine.AI.NavMesh.SamplePosition(Triangulation.vertices[vertexIndex], out Hit, 2f, 1))
            {
                //enemy finds the NavMeshAgent and attaches itself properly
                enemy.agent.Warp(Hit.position);
                //the player on the NavMesh has their transform component shared with the enemy movement script's player transform
                enemy.movement.player = Player;
                //enemy on NavMeshAgent is enabled
                enemy.agent.enabled = true;
                //enemy moves to chase the player
                enemy.movement.StartChasing();
            }
            else
            {
                Debug.LogError("Cannot place agent on NavMesh. Attempted " + Triangulation.vertices[vertexIndex]);
            }
        }
        else
        {
            Debug.LogError("Unable to fetch enemy of type " + SpawnIndex + " from object pool. Check if you're out of objects.");
        }
    }
}
