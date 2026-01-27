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

    private bool deathSequenceActive;

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
        StartCoroutine(Initialize());
    }

    private IEnumerator Initialize()
    {
        while (AudioManager.Instance == null || DialogueManager.Instance == null)
            yield return null;

        AudioManager.Instance.PlayIntroThenLoop(MusicState.Intro, MusicState.LoopA);
        dialogueManager = DialogueManager.Instance;
        StartCoroutine(LevelIntroSequence());
    }

    public static void TriggerPlayerDeath()
    {
        OnPlayerDeathEvent?.Invoke();

        if (Instance != null)
        {
            Instance.BeginDeathSequence();
            return;
        }

        var manager = FindObjectOfType<LevelManager>(true);
        if (manager != null)
        {
            Instance = manager;
            manager.BeginDeathSequence();
        }
    }

    private void BeginDeathSequence()
    {
        if (deathSequenceActive) return;
        deathSequenceActive = true;
        StartCoroutine(HandlePlayerDeath());
    }

    private IEnumerator HandlePlayerDeath()
    {
        if (DialogueManager.Instance != null)
            DialogueManager.Instance.ResetDialogue();

        if (AudioManager.Instance != null && deathSound != null)
            AudioManager.Instance.PlaySFX(deathSound.clip);
        else if (deathSound != null)
        {
            deathSound.Stop();
            deathSound.Play();
        }

        if (fadeCanvas != null)
            fadeCanvas.alpha = 1f;

        var ui = FindObjectOfType<DeathScreenUI>(true);
        if (ui != null)
            ui.Show();

        Time.timeScale = 0f;

        yield break;
    }

    private IEnumerator LevelIntroSequence()
    {
        yield return dialogueManager.InstructionalText("Stone tablets are near...they should help lead the way.", 3f);
    }
}
