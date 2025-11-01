using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class PlayerRespawn : MonoBehaviour
{
    public static PlayerRespawn Instance { get; private set; }

    private Vector3 respawnPoint;
    private string currentScene;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void Start()
    {
        StartCoroutine(InitializeSpawn());
    }

    private IEnumerator InitializeSpawn()
    {
        yield return new WaitForFixedUpdate();

        var player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            respawnPoint = player.transform.position;

        currentScene = SceneManager.GetActiveScene().name;
    }

    public void SetRespawnPoint(Vector3 newRespawnPoint)
    {
        respawnPoint = newRespawnPoint;
        currentScene = SceneManager.GetActiveScene().name;
    }

    public void RespawnPlayer()
    {
        SceneManager.LoadScene(currentScene);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        StartCoroutine(RespawnAfterSceneLoad());
    }

    private IEnumerator RespawnAfterSceneLoad()
    {
        // Wait 2 frames to ensure colliders and physics are ready
        yield return null;
        yield return new WaitForFixedUpdate();

        var player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) yield break;

        if (player.TryGetComponent(out Rigidbody rb))
        {
            // Disable gravity and temporarily freeze motion
            rb.useGravity = false;
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            // Move the player slightly above ground
            player.transform.SetPositionAndRotation(respawnPoint + Vector3.up * 0.2f, Quaternion.identity);

            yield return new WaitForFixedUpdate(); // let physics update once

            // Re-enable gravity after reposition
            rb.useGravity = true;
        }
        else
        {
            player.transform.SetPositionAndRotation(respawnPoint + Vector3.up * 0.2f, Quaternion.identity);
        }

        var health = player.GetComponent<PlayerHealth>();
        if (health != null)
            health.ResetHealth();
    }
}
