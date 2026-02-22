using System.Collections;
using UnityEngine;

public class PlayerRespawn : MonoBehaviour
{
    public static PlayerRespawn Instance { get; private set; }

    public bool IsRespawning { get; private set; }

    private Vector3 respawnPosition;
    private Quaternion respawnRotation;

    private Coroutine respawnRoutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        StartCoroutine(InitializeRespawnData());
    }

    private IEnumerator InitializeRespawnData()
    {
        GameObject player = null;

        while (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player");
            yield return null;
        }

        yield return new WaitForFixedUpdate();

        respawnPosition = player.transform.position;
        respawnRotation = player.transform.rotation;
    }

    public void SetRespawnPoint(Vector3 position, Quaternion rotation)
    {
        respawnPosition = position;
        respawnRotation = rotation;
    }

    public void RespawnPlayer()
    {
        if (respawnRoutine != null)
            StopCoroutine(respawnRoutine);

        respawnRoutine = StartCoroutine(RespawnRoutine());
    }

    private IEnumerator RespawnRoutine()
    {
        IsRespawning = true;
        Time.timeScale = 1f;

        GameObject player = null;
        while (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player");
            yield return null;
        }

        yield return null;
        yield return new WaitForFixedUpdate();

        Vector3 finalPosition = respawnPosition + Vector3.up * 0.2f;
        Quaternion finalRotation = respawnRotation;

        if (player.TryGetComponent(out Rigidbody rb))
        {
            rb.isKinematic = false;
            rb.useGravity = true;
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            player.transform.SetPositionAndRotation(finalPosition, finalRotation);

            yield return new WaitForFixedUpdate();

            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.WakeUp();
        }
        else
        {
            player.transform.SetPositionAndRotation(finalPosition, finalRotation);
            yield return new WaitForFixedUpdate();
        }

        HealthManager health = player.GetComponent<HealthManager>();
        if (health != null)
            health.RestoreFullHealth();

        IsRespawning = false;
        respawnRoutine = null;
    }
}