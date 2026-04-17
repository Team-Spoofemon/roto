using UnityEngine;

public class CameraOrientation : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private Transform[] orientationTargets;
    [SerializeField] private LayerMask orientationMask;
    [SerializeField] private float rayDistance = 200f;
    [SerializeField] private float rotateSpeed = 8f;
    [SerializeField] private Vector3 rayOffset = new Vector3(0f, 1f, 0f);

    private void LateUpdate()
    {
        if (player == null || orientationTargets == null || orientationTargets.Length == 0) return;

        Transform bestTarget = null;
        float bestDistanceSqr = float.MaxValue;
        Vector3 origin = player.position + rayOffset;

        for (int i = 0; i < orientationTargets.Length; i++)
        {
            Transform currentTarget = orientationTargets[i];
            if (currentTarget == null) continue;

            Vector3 toTarget = currentTarget.position - origin;
            float distanceToTarget = toTarget.magnitude;
            if (distanceToTarget <= 0.0001f) continue;
            if (distanceToTarget > rayDistance) continue;

            Vector3 rayDirection = toTarget.normalized;
            Debug.DrawRay(origin, rayDirection * distanceToTarget, Color.red);

            if (!Physics.Raycast(origin, rayDirection, out RaycastHit hit, distanceToTarget, orientationMask)) continue;

            if (hit.transform != currentTarget) continue;

            float distanceSqr = (currentTarget.position - player.position).sqrMagnitude;
            if (distanceSqr < bestDistanceSqr)
            {
                bestDistanceSqr = distanceSqr;
                bestTarget = currentTarget;
            }
        }

        if (bestTarget == null) return;

        Vector3 forward = bestTarget.position - player.position;
        forward = Vector3.ProjectOnPlane(forward, Vector3.up);

        if (forward.sqrMagnitude <= 0.0001f) return;

        Quaternion targetRotation = Quaternion.LookRotation(forward.normalized, Vector3.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotateSpeed * Time.deltaTime);
    }
}