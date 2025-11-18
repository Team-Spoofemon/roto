using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    [SerializeField] private GameObject enemyPrefabs;
    [SerializeField] private GameObject enemyContainer;

    IEnumerator SpawnRoutine()
    {
        while (true)
        {
            Vector3 spawnHere = new Vector3(Random.Range(-8f, 8f), 7, 0);
            GameObject newEnemy = Instantiate(enemyPrefabs, spawnHere, Quaternion.identity);
            newEnemy.transform.parent = enemyContainer.transform;
            yield return new WaitForSeconds(5.0f);
        }
    }

    void Start()
        {
            StartCoroutine(SpawnRoutine());
        }
}
