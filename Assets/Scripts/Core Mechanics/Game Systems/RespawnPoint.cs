/// <summary>
/// Defines a checkpoint that updates the player respawn location when entered.
/// Uses a trigger collider and supports multiple coordinate sources for flexibility.
///
/// HOW TO USE
/// - Add this component to a GameObject with a BoxCollider set as Trigger.
/// - Place the object at a checkpoint location in the scene.
/// - The Player object must use the "Player" tag.
///
/// COORDINATE SOURCE
/// - TransformPosition: uses the GameObject transform position.
/// - ColliderPosition: uses the center of the collider bounds.
/// - Vector3Variable: uses a manually assigned Vector3 value.
///
/// NOTES
/// - Requires PlayerRespawn to exist and be initialized.
/// - Trigger activation updates the respawn point immediately.
/// </summary>


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

        Vector3 pos;
        switch (coordinateSource)
        {
            default:
            case CoordinateSource.TransformPosition:
                pos = transform.position;
                break;
            case CoordinateSource.ColliderPosition:
                pos = GetComponent<Collider>().bounds.center;
                break;
            case CoordinateSource.Vector3Variable:
                pos = respawnCoordinates;
                break;
        }

        PlayerRespawn.Instance.SetRespawnPoint(pos, transform.rotation);
    }
}