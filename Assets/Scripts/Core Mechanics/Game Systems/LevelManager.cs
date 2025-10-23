using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }
    private CombatManager combatManager;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            GenerateManager<CombatManager>(ref combatManager);
            DontDestroyOnLoad(this.gameObject);
        }
    }

    private IEnumerator Start()
    {
        yield return new WaitUntil(() => GameObject.FindGameObjectWithTag("Player") != null);
        BindPlayer();
    }

    private void GenerateManager<T>(ref T comp)
        where T : Component
    {
        comp = GetComponent<T>();
        if (comp == null)
        {
            comp = gameObject.AddComponent<T>();
            Debug.Log($"{typeof(T).Name} created in the GameManager!");
        }
    }

    // redesign the below code to event based
    private PlayerHealth playerHealth;
    private PlayerRespawn playerRespawn;
    private Vector3 checkpointPosition = Vector3.zero;

    private void BindPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerHealth = player.GetComponent<PlayerHealth>();
            playerRespawn = player.GetComponent<PlayerRespawn>();
            if (playerRespawn != null)
                checkpointPosition = playerRespawn.GetRespawnPoint();
        }
    }

    public void OnPlayerDeath()
    {
        if (playerRespawn != null)
            checkpointPosition = playerRespawn.GetRespawnPoint();
        StartCoroutine(RestartFromCheckpoint());
    }

    private IEnumerator RestartFromCheckpoint()
    {
        Scene currentScene = SceneManager.GetActiveScene();
        yield return SceneManager.LoadSceneAsync(currentScene.name);
        yield return new WaitUntil(() => GameObject.FindGameObjectWithTag("Player") != null);
        BindPlayer();

        if (playerRespawn != null)
        {
            playerRespawn.SetRespawnPoint(checkpointPosition);
            playerRespawn.Respawn(playerRespawn.transform);
        }
    }
}
