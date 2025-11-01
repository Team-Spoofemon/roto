using UnityEngine;
using System;
using System.Collections;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    public static event Action OnPlayerDeathEvent;

    [SerializeField] private float respawnDelay = 1.5f;
    [SerializeField] private CanvasGroup fadeCanvas;
    [SerializeField] private AudioSource deathSound;

    private void OnEnable()
    {
        OnPlayerDeathEvent += HandlePlayerDeathTrigger;
    }

    private void OnDisable()
    {
        OnPlayerDeathEvent -= HandlePlayerDeathTrigger;
    }

    private void HandlePlayerDeathTrigger()
    {
        StartCoroutine(HandlePlayerDeath());
    }

    private IEnumerator HandlePlayerDeath()
    {
        if (deathSound != null)
            deathSound.Play();

        if (fadeCanvas != null)
            yield return StartCoroutine(FadeOut());

        yield return new WaitForSeconds(respawnDelay);

        if (PlayerRespawn.Instance != null)
            PlayerRespawn.Instance.RespawnPlayer();
    }

    private IEnumerator FadeOut()
    {
        float duration = 1f;
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            fadeCanvas.alpha = Mathf.Lerp(0, 1, t / duration);
            yield return null;
        }
    }
}
