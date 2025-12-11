using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    [System.Serializable]
    public class EnemyPool
    {
        public GameObject enemyPrefab;
        public int poolSize = 10;
        [HideInInspector] public List<GameObject> pool = new List<GameObject>();
    }

    [SerializeField] private List<EnemyPool> enemyPools = new List<EnemyPool>();
    [SerializeField] private GameObject enemyContainer;

    private void Start()
    {
        StartCoroutine(InitializePools());
    }

    private IEnumerator InitializePools()
    {
        yield return new WaitForEndOfFrame();

        foreach (EnemyPool pool in enemyPools)
        {
            for (int i = 0; i < pool.poolSize; i++)
            {
                GameObject enemy = Instantiate(pool.enemyPrefab, enemyContainer.transform);
                enemy.SetActive(false);
                pool.pool.Add(enemy);
            }
        }
    }

    private GameObject GetPooledEnemy(EnemyPool pool)
    {
        foreach (GameObject enemy in pool.pool)
        {
            if (!enemy.activeInHierarchy)
                return enemy;
        }
        GameObject newEnemy = Instantiate(pool.enemyPrefab, enemyContainer.transform);
        newEnemy.SetActive(false);
        pool.pool.Add(newEnemy);
        return newEnemy;
    }

    private GameObject SpawnEnemy(Vector3 position)
    {
        EnemyPool selectedPool = enemyPools[Random.Range(0, enemyPools.Count)];
        GameObject enemy = GetPooledEnemy(selectedPool);
        enemy.transform.position = position;
        enemy.SetActive(true);
        return enemy;
    }

    public void SpawnInArea(Vector3 center, float radius)
    {
        int count = Random.Range(0, 5);
        for (int i = 0; i < count; i++)
        {
            Vector3 spawnPos = center + Random.insideUnitSphere * radius;
            spawnPos.z = center.z;
            SpawnEnemy(spawnPos);
        }
    }
}
