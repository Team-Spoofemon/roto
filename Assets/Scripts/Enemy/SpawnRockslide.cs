using System.Collections;
using UnityEngine;
using Cinemachine;

public class SpawnRockslide : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private Transform mainCamera;
    [SerializeField] private Transform rockslideZone;
    [SerializeField] private Transform orientationTarget;
    [SerializeField] private GameObject rockslidePrefab;
    [SerializeField] private CinemachineImpulseSource impulse;

    [Header("Zone Follow")]
    [SerializeField] private float zoneFollowDistanceFromPlayer = 6f;
    [SerializeField] private float zoneVerticalOffset = 12f;
    [SerializeField] private float zoneRayStartHeight = 30f;
    [SerializeField] private float zoneRayDistance = 150f;
    [SerializeField] private LayerMask cameraOrientationLayer;

    [Header("Spawn")]
    [SerializeField] private float terrainRayStartHeight = 30f;
    [SerializeField] private float terrainRayDistance = 200f;
    [SerializeField] private float spawnHeightAboveGround = 8f;
    [SerializeField] private float spawnForwardOffset = 5f;
    [SerializeField] private LayerMask terrainMask;

    [Header("Release")]
    [SerializeField] private float releaseDelay = 0.05f;
    [SerializeField] private float slideSpeed = 18f;
    [SerializeField] private float stickForce = 4f;

    [Header("Damage")]
    [SerializeField] private float rockDamage = 10f;

    [Header("Debug")]
    [SerializeField] private bool debugLogs = true;

    private void Awake()
    {
        if (mainCamera == null && Camera.main != null)
            mainCamera = Camera.main.transform;
    }

    private void LateUpdate()
    {
        UpdateRockslideZone();
    }

    public void Spawn()
    {
        Log("Spawn called.");

        if (rockslideZone == null || rockslidePrefab == null)
        {
            Log("Spawn failed: missing rockslideZone or rockslidePrefab.");
            return;
        }

        Vector3 rayStart = rockslideZone.position + Vector3.up * terrainRayStartHeight;

        if (!Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, terrainRayDistance, terrainMask, QueryTriggerInteraction.Ignore))
        {
            Log("Spawn failed: no Terrain found below zone.");
            return;
        }

        Vector3 downSlope = GetDownSlope(hit.normal);
        if (downSlope.sqrMagnitude <= 0.0001f)
        {
            Log("Spawn failed: downslope too small.");
            return;
        }

        Vector3 forwardOffset = Vector3.ProjectOnPlane(rockslideZone.forward, Vector3.up).normalized * spawnForwardOffset;
        Vector3 spawnPosition = hit.point + forwardOffset + Vector3.up * spawnHeightAboveGround;

        if (impulse != null)
            impulse.GenerateImpulse(Vector3.down);

        GameObject rockslide = Instantiate(
            rockslidePrefab,
            spawnPosition,
            rockslideZone.rotation
        );

        Destroy(rockslide, 20f);

        AddRockDamageComponents(rockslide);

        Log("Rockslide spawned at " + rockslide.transform.position);
        StartCoroutine(ReleaseRockslide(rockslide, downSlope, hit.normal));
    }

    private void UpdateRockslideZone()
    {
        if (player == null || rockslideZone == null || orientationTarget == null)
        {
            Log("Zone update skipped: missing player, rockslideZone, or orientationTarget.");
            return;
        }

        Vector3 forward = orientationTarget.position - player.position;
        forward = Vector3.ProjectOnPlane(forward, Vector3.up);

        if (forward.sqrMagnitude <= 0.0001f)
        {
            Log("Zone update skipped: forward too small.");
            return;
        }

        forward.Normalize();

        Vector3 desiredPosition = player.position + forward * zoneFollowDistanceFromPlayer;
        desiredPosition.y = player.position.y + zoneVerticalOffset;

        Vector3 rayStart = desiredPosition + Vector3.up * zoneRayStartHeight;

        if (Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, zoneRayDistance, cameraOrientationLayer, QueryTriggerInteraction.Ignore))
        {
            rockslideZone.position = hit.point;
        }
        else
        {
            rockslideZone.position = desiredPosition;
        }

        if (mainCamera != null)
        {
            Vector3 toCamera = mainCamera.position - rockslideZone.position;
            toCamera = Vector3.ProjectOnPlane(toCamera, Vector3.up);

            if (toCamera.sqrMagnitude > 0.0001f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(toCamera.normalized, Vector3.up);
                rockslideZone.rotation = Quaternion.Slerp(
                    rockslideZone.rotation,
                    targetRotation,
                    zoneFollowDistanceFromPlayer * Time.deltaTime
                );
            }
        }

        Log("Zone updated to " + rockslideZone.position);
    }

    private void AddRockDamageComponents(GameObject rockslide)
    {
        Rigidbody[] bodies = rockslide.GetComponentsInChildren<Rigidbody>(true);

        for (int i = 0; i < bodies.Length; i++)
        {
            Rigidbody rb = bodies[i];
            if (rb == null)
                continue;

            RockslideDamage damage = rb.GetComponent<RockslideDamage>();
            if (damage == null)
                damage = rb.gameObject.AddComponent<RockslideDamage>();

            damage.SetDamage(rockDamage);
        }
    }

    private IEnumerator ReleaseRockslide(GameObject rockslide, Vector3 downSlope, Vector3 surfaceNormal)
    {
        yield return new WaitForSeconds(releaseDelay);

        Transform container1 = rockslide.transform.Find("Tier 1/Container 1");
        Transform container2 = rockslide.transform.Find("Tier 2/Container 2");

        if (container1 != null)
            container1.gameObject.SetActive(false);
        else
            Log("Could not find Tier 1/Container 1");

        if (container2 != null)
            container2.gameObject.SetActive(false);
        else
            Log("Could not find Tier 2/Container 2");

        Rigidbody[] bodies = rockslide.GetComponentsInChildren<Rigidbody>();

        for (int i = 0; i < bodies.Length; i++)
        {
            Rigidbody rb = bodies[i];
            if (rb == null)
                continue;

            rb.velocity = downSlope * slideSpeed - surfaceNormal * stickForce;
        }
    }

    private Vector3 GetDownSlope(Vector3 normal)
    {
        return Vector3.ProjectOnPlane(Vector3.down, normal).normalized;
    }

    private void Log(string message)
    {
        if (debugLogs)
            Debug.Log("[SpawnRockslide] " + message, this);
    }
}

public class RockslideDamage : MonoBehaviour
{
    private float damage = 10f;
    private float hitCooldown = 0.2f;
    private float lastHitTime = -999f;

    public void SetDamage(float value)
    {
        damage = value;
    }

    private void OnCollisionEnter(Collision collision)
    {
        TryDamage(collision.transform);
    }

    private void OnTriggerEnter(Collider other)
    {
        TryDamage(other.transform);
    }

    private void TryDamage(Transform other)
    {
        if (Time.time - lastHitTime < hitCooldown)
            return;

        Transform root = other.root;
        if (!root.CompareTag("Player"))
            return;

        HealthManager health = root.GetComponentInChildren<HealthManager>();
        if (health == null)
            return;

        health.TakeDamage(damage);
        lastHitTime = Time.time;
    }
}