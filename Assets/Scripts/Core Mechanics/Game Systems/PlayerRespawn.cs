using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerRespawn : MonoBehaviour
{
    public static PlayerRespawn Instance { get; private set; }

    private Vector3 respawnPosition;
    private Quaternion respawnRotation;
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

        respawnPosition = player.transform.position;
        respawnRotation = player.transform.rotation;
        currentScene = player.scene.name;
    }

    public void SetRespawnPoint(Vector3 position, Quaternion rotation)
    {
        respawnPosition = position;
        respawnRotation = rotation;

        var player = GameObject.FindGameObjectWithTag("Player");
        currentScene = player != null ? player.scene.name : SceneManager.GetActiveScene().name;
    }

    public void RespawnPlayer()
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        currentScene = player != null ? player.scene.name : SceneManager.GetActiveScene().name;

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

        var targetPos = respawnPosition + Vector3.up * 0.2f;
        var targetRot = respawnRotation;

        if (player.TryGetComponent(out Rigidbody rb))
        {
            rb.useGravity = false;
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            player.transform.SetPositionAndRotation(targetPos, targetRot);

            yield return new WaitForFixedUpdate();

            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.useGravity = true;
        }
        else
        {
            player.transform.SetPositionAndRotation(targetPos, targetRot);
        }

        var healthManager = player.GetComponent<HealthManager>();
        if (healthManager != null)
        {
            healthManager.RestoreFullHealth();
        }

        respawnRequested = false;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}