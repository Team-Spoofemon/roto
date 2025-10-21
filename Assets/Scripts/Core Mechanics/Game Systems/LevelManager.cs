using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }
    private PlayerHealth playerHealth;
    private PlayerRespawn playerRespawn;

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

    private IEnumerator Start()
    {
        yield return new WaitUntil(() => GameObject.FindGameObjectWithTag("Player") != null);
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        playerHealth = player.GetComponent<PlayerHealth>();
        playerRespawn = player.GetComponent<PlayerRespawn>();
    }

    public void OnPlayerDeath()
    {
        StartCoroutine(RestartFromCheckpoint());
    }

    private IEnumerator RestartFromCheckpoint()
    {
        Vector3 checkpoint = Vector3.zero;
        if (playerRespawn != null)
            checkpoint = playerRespawn.GetRespawnPoint();

        Scene currentScene = SceneManager.GetActiveScene();
        yield return SceneManager.LoadSceneAsync(currentScene.name);
        yield return null;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerRespawn = player.GetComponent<PlayerRespawn>();
            playerRespawn.SetRespawnPoint(checkpoint);
            player.transform.SetPositionAndRotation(checkpoint, Quaternion.identity);
        }
    }

}
