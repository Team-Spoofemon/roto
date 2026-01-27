using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerRespawn : MonoBehaviour
{
    public static PlayerRespawn Instance { get; private set; }

    private Vector3 respawnPoint;
    private string currentScene;
    private bool respawnRequested;

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
        GameObject player = null;
        while (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player");
            yield return null;
        }

        yield return new WaitForFixedUpdate();

        respawnPoint = player.transform.position;
        currentScene = player.scene.name;
    }

    public void SetRespawnPoint(Vector3 newRespawnPoint)
    {
        respawnPoint = newRespawnPoint;

        var player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            currentScene = player.scene.name;
    }

    public void RespawnPlayer()
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            currentScene = player.scene.name;

        if (string.IsNullOrEmpty(currentScene))
            currentScene = SceneManager.GetActiveScene().name;

        respawnRequested = true;
        Time.timeScale = 1f;
        SceneManager.LoadScene(currentScene);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!respawnRequested) return;
        if (scene.name != currentScene) return;

        StartCoroutine(RespawnAfterSceneLoad());
    }

    private IEnumerator RespawnAfterSceneLoad()
    {
        yield return null;
        yield return new WaitForFixedUpdate();

        var player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            respawnRequested = false;
            yield break;
        }

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

        var healthManager = player.GetComponent<HealthManager>();
        if (healthManager != null)
        {
            var totalHealthField = typeof(HealthManager).GetField(
                "totalHealth",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance
            );

            var currentHealthField = typeof(HealthManager).GetField(
                "currentHealth",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance
            );

            if (totalHealthField != null && currentHealthField != null)
            {
                float totalHealth = (float)totalHealthField.GetValue(healthManager);
                currentHealthField.SetValue(healthManager, totalHealth);
            }
        }

        respawnRequested = false;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}
