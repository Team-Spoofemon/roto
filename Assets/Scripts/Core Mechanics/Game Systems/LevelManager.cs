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
        StartCoroutine(HandlePlayerDeath());
    }

    private IEnumerator HandlePlayerDeath()
    {
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

        DeathScreenUI ui = FindObjectOfType<DeathScreenUI>(true);
        if (ui != null)
            ui.Show();

        yield break;
    }

    private IEnumerator LevelIntroSequence()
    {
        yield return dialogueManager.InstructionalText("See Adrasteia at the center of the map.", 3f);
    }
}
