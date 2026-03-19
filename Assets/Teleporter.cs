using UnityEngine;

public class Teleporter : MonoBehaviour
{
    [SerializeField] private Transform teleportFrom;
    [SerializeField] private Transform teleportTo;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (teleportFrom == null || teleportTo == null) return;

        if (other.transform == teleportFrom || other.transform.root == teleportFrom)
        {
            other.transform.root.position = teleportTo.position;
            other.transform.root.rotation = teleportTo.rotation;
        }
    }
}