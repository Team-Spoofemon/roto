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
        yield return null;
        yield return new WaitForFixedUpdate();

        var player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) yield break;

        if (player.TryGetComponent(out Rigidbody rb))
        {
            rb.useGravity = false;
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            player.transform.SetPositionAndRotation(respawnPoint + Vector3.up * 0.2f, Quaternion.identity);

            yield return new WaitForFixedUpdate();

            rb.useGravity = true;
        }
        else
        {
            player.transform.SetPositionAndRotation(respawnPoint + Vector3.up * 0.2f, Quaternion.identity);
        }

        // Updated section — use HealthManager instead of PlayerHealth
        var healthManager = player.GetComponent<HealthManager>();
        if (healthManager != null)
        {
            // Reset stats manually
            var totalHealthField = typeof(HealthManager)
                .GetField("totalHealth", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var currentHealthField = typeof(HealthManager)
                .GetField("currentHealth", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            if (totalHealthField != null && currentHealthField != null)
            {
                float totalHealth = (float)totalHealthField.GetValue(healthManager);
                currentHealthField.SetValue(healthManager, totalHealth);
            }
        }
    }
}
