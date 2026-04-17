using UnityEngine;

public class RockslideTrigger : MonoBehaviour
{
    [SerializeField] private SpawnRockslide rockslide;
    [SerializeField] private bool triggerOnce = true;

    private bool hasTriggered;

    private void OnTriggerEnter(Collider other)
    {
        if (triggerOnce && hasTriggered)
            return;

        Transform root = other.transform.root;

        if (!root.CompareTag("Player"))
            return;

        if (rockslide == null)
            return;

        rockslide.Spawn();
        hasTriggered = true;
    }
}