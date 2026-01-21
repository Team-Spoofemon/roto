using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class SpawnZone : MonoBehaviour
{
    /*private SpawnManager _spawnManager;
    private SphereCollider sphereCollider;
    private bool playerInside = false;
    private bool hasSpawned = false;

    private void Start()
    {
        _spawnManager = FindObjectOfType<SpawnManager>();
        sphereCollider = GetComponent<SphereCollider>();
        sphereCollider.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !playerInside && !hasSpawned)
        {
            playerInside = true;
            hasSpawned = true;
            _spawnManager.SpawnInArea(transform.position, sphereCollider.radius * transform.localScale.x);
            gameObject.SetActive(false);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInside = false;
        }
    }*/
}
