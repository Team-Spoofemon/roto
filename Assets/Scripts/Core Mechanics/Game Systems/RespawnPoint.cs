using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class RespawnPoint : MonoBehaviour
{
    private enum CoordinateSource
    {
        TransformPosition,
        ColliderPosition,
        Vector3Variable,
    }

    [SerializeField] private CoordinateSource coordinateSource;
    [SerializeField] private Vector3 respawnCoordinates;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        switch (coordinateSource)
        {
            default:
            case CoordinateSource.TransformPosition:
                PlayerRespawn.Instance.SetRespawnPoint(transform.position);
                break;
            case CoordinateSource.ColliderPosition:
                PlayerRespawn.Instance.SetRespawnPoint(GetComponent<Collider>().bounds.center);
                break;
            case CoordinateSource.Vector3Variable:
                PlayerRespawn.Instance.SetRespawnPoint(respawnCoordinates);
                break;
        }
    }
}
