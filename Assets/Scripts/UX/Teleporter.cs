using UnityEngine;
using System.Collections;

public class Teleporter : MonoBehaviour
{
    [SerializeField] private Transform destination;
    [SerializeField] private float cooldown = 0.2f;

    private bool canTeleport = true;

    private void OnTriggerEnter(Collider other)
    {
        if (!canTeleport) return;
        if (!other.transform.root.CompareTag("Player")) return;
        if (destination == null) return;

        Transform playerRoot = other.transform.root;

        Collider destCollider = destination.GetComponent<Collider>();

        playerRoot.position = destination.position;
        playerRoot.rotation = destination.rotation;

        if (destCollider != null)
        {
            StartCoroutine(DisableDestinationCollider(destCollider));
        }
    }

    private IEnumerator DisableDestinationCollider(Collider col)
    {
        canTeleport = false;

        col.enabled = false;

        yield return new WaitForSeconds(cooldown);

        col.enabled = true;
        canTeleport = true;
    }
}