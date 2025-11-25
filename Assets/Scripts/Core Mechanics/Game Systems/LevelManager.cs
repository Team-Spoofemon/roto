using System;
using System.Collections;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static event Action OnPlayerDeathEvent;

    [SerializeField] private CanvasGroup fadeCanvas;
    [SerializeField] private AudioSource deathSound;

    private DialogueManager dialogueManager;

    public static LevelManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        dialogueManager = DialogueManager.Instance;
        StartCoroutine(LevelIntroSequence());
    }

    private void OnEnable()
    {
        OnPlayerDeathEvent += HandlePlayerDeathTrigger;
    }

    private void OnDisable()
    {
        OnPlayerDeathEvent -= HandlePlayerDeathTrigger;
    }

    public static void TriggerPlayerDeath()
    {
        OnPlayerDeathEvent?.Invoke();
    }

    private void HandlePlayerDeathTrigger()
    {
        if (Instance != null)
            Instance.StartCoroutine(Instance.HandlePlayerDeath());
    }

    private IEnumerator HandlePlayerDeath()
    {
        if (AudioManager.Instance != null && deathSound != null)
            AudioManager.Instance.PlaySFX(deathSound.clip);
        else if (deathSound != null)
        {
            deathSound.Stop();
            deathSound.Play();
        }

        if (fadeCanvas != null)
            fadeCanvas.alpha = 0;

        DeathScreenUI deathScreen = FindObjectOfType<DeathScreenUI>(true);
        if (deathScreen != null)
            deathScreen.Show();

        yield break;
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

    private IEnumerator LevelIntroSequence()
    {
        dialogueManager.InstructionalText("See Ada at the center of the map.");
        yield return new WaitForSeconds(3f);
        dialogueManager.HideText();
    }
}
