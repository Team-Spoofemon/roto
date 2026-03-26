using UnityEngine;

public class CameraOrientation : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private Transform orientationTarget;
    [SerializeField] private LayerMask orientationMask;
    [SerializeField] private float rayDistance = 200f;
    [SerializeField] private float rotateSpeed = 8f;
    [SerializeField] private Vector3 rayOffset = new Vector3(0f, 1f, 0f);

    private void LateUpdate()
    {
    if (player == null || orientationTarget == null) return;

    Vector3 origin = player.position + rayOffset;
    Vector3 rayDirection = (orientationTarget.position - origin).normalized;

    Debug.DrawRay(origin, rayDirection * rayDistance, Color.red);

    if (!Physics.Raycast(origin, rayDirection, out RaycastHit hit, rayDistance, orientationMask)) return;

    Vector3 forward = orientationTarget.position - player.position;
    forward = Vector3.ProjectOnPlane(forward, Vector3.up);

    if (forward.sqrMagnitude <= 0.0001f) return;

    Quaternion targetRotation = Quaternion.LookRotation(forward.normalized, Vector3.up);
    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotateSpeed * Time.deltaTime);
    }
}