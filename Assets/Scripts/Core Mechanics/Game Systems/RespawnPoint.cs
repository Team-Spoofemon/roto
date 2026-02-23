using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class RespawnPoint : MonoBehaviour
{
    [SerializeField] private bool useColliderCenter = true;
    [SerializeField] private Vector3 respawnCoordinates;

    private BoxCollider box;

    private void Awake()
    {
        box = GetComponent<BoxCollider>();
        box.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (PlayerRespawn.Instance == null)
            return;

        Vector3 pos;
        if (useColliderCenter)
            pos = box.bounds.center;
        else
            pos = respawnCoordinates;

        PlayerRespawn.Instance.SetRespawnPoint(pos, transform.rotation);
        Debug.Log("Respawn point set: " + pos + " in scene: " + gameObject.scene.name);
    }
}