// LevelManager.cs
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
        Debug.Log("LEVELMANAGER TriggerPlayerDeath() entered");

        try
        {
            OnPlayerDeathEvent?.Invoke();
        }
        catch (Exception e)
        {
            Debug.LogError("LEVELMANAGER OnPlayerDeathEvent listener threw: " + e);
        }

        if (Instance != null)
            Instance.BeginDeathSequence();
    }

    private void BeginDeathSequence()
    {
        Debug.Log("LEVELMANAGER BeginDeathSequence() deathSequenceActive=" + deathSequenceActive);

        if (deathSequenceActive)
            return;

        deathSequenceActive = true;
        StartCoroutine(HandlePlayerDeath());
    }

    private IEnumerator HandlePlayerDeath()
    {
        Debug.Log("LEVELMANAGER HandlePlayerDeath() started - showing death UI");

        if (DialogueManager.Instance != null)
            DialogueManager.Instance.ResetDialogue();

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayDeathTheme(0.15f, 0.15f);
            if (deathSound != null) AudioManager.Instance.PlaySFX(deathSound.clip);
        }
        else if (deathSound != null)
        {
            deathSound.Stop();
            deathSound.Play();
        }

        if (fadeCanvas != null)
            fadeCanvas.alpha = 1f;

        if (DeathScreenUI.Instance != null)
            DeathScreenUI.Instance.Show();
        else
            Debug.LogError("LEVELMANAGER: DeathScreenUI.Instance is null (bootstrap UI not present)");

        Time.timeScale = 0f;
        yield break;
    }

    public void RespawnFromDeathScreen()
    {
        Time.timeScale = 1f;
        deathSequenceActive = false;

        if (DeathScreenUI.Instance != null)
            DeathScreenUI.Instance.Hide();

        if (PlayerRespawn.Instance != null)
            PlayerRespawn.Instance.RespawnPlayer();
    }

    private IEnumerator LevelIntroSequence()
    {
        yield return dialogueManager.InstructionalText("Stone tablets are near...they should help lead the way.", 3f);
    }
}