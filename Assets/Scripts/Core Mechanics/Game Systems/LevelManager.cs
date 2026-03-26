using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class LevelManager : MonoBehaviour
{
    public static event Action OnPlayerDeathEvent;
    public static LevelManager Instance { get; private set; }

    [Header("UI")]
    [SerializeField] private CanvasGroup fadeCanvas;

    [Header("Player")]
    [SerializeField] private GameObject playerRoot;

    [Header("Player")]
    [SerializeField] private PlayerController playerController;
    [SerializeField] private PlayerCombat playerCombat;
    [SerializeField] private PlayerInput playerInput;

    [Header("Physics Lock")]
    [SerializeField] private Rigidbody playerRigidbody;

    [Header("Spawn")]
    public Transform SpawnStart;

    [Header("Audio")]
    [SerializeField] private AudioSource deathSound;
    [SerializeField] private float levelCompleteFadeOutSeconds = 0.6f;

    [Header("Level Complete Fade")]
    [SerializeField] private float levelCompleteFadeToBlackSeconds = 0.2f;

    [Header("Level Music")]
    [SerializeField] private bool playLevelMusic = true;
    [SerializeField] private bool hasIntroMusic = true;
    [SerializeField] private MusicState introMusic = MusicState.Intro;
    [SerializeField] private MusicState loopMusic = MusicState.LoopA;

    [Header("Realm")]
    [SerializeField] private RealmType currentRealm;
    public RealmType CurrentRealm => currentRealm;
    [SerializeField] private RealmType nextRealm = RealmType.CreteValley;

    [Header("Level Intro Text")]
    [SerializeField] private bool showIntroText = true;
    [SerializeField] private string introText = "Stone tablets are near...they should help lead the way.";
    [SerializeField] private float introTextSeconds = 3f;

    [Header("Level Flyover")]
    [SerializeField] private bool playIntroFlyover = true;
    [SerializeField] private LevelFlyover levelFlyover;

    private DialogueManager dialogueManager;

    private bool deathSequenceActive;
    private bool levelCompleteSequenceActive;

    private string savedActionMapName;

    private void Awake()
    {
        if (Instance != null && Instance != this)
            Instance.enabled = false;

        Instance = this;
        ResolvePlayerRefs();
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    private void Start()
    {
        StartCoroutine(Initialize());
    }

    private void ResolvePlayerRefs()
    {
        playerRoot = null;
        playerController = null;
        playerCombat = null;
        playerInput = null;
        playerRigidbody = null;

        GameObject tagged = GameObject.FindGameObjectWithTag("Player");
        if (tagged != null)
            playerRoot = tagged;

        if (playerRoot == null)
            return;

        playerInput = playerRoot.GetComponentInChildren<PlayerInput>(true);
        playerCombat = playerRoot.GetComponentInChildren<PlayerCombat>(true);
        playerRigidbody = playerRoot.GetComponentInChildren<Rigidbody>(true);
        playerController = playerRoot.GetComponentInChildren<PlayerController>(true);
    }

    private void FreezePhysics()
    {
        if (playerRigidbody == null)
            return;

        playerRigidbody.useGravity = false;
        playerRigidbody.isKinematic = true;
        playerRigidbody.Sleep();
    }

    private void RestorePhysics()
    {
        if (playerRigidbody == null)
            return;

        playerRigidbody.isKinematic = false;
        playerRigidbody.useGravity = true;
        playerRigidbody.velocity = Vector3.zero;
        playerRigidbody.angularVelocity = Vector3.zero;
        playerRigidbody.WakeUp();
    }

    private void SaveCurrentActionMap()
    {
        if (playerInput == null || playerInput.currentActionMap == null)
        {
            savedActionMapName = null;
            return;
        }

        string mapName = playerInput.currentActionMap.name;

        if (string.Equals(mapName, "UI", StringComparison.OrdinalIgnoreCase))
            return;

        savedActionMapName = mapName;
    }

    private void RestoreActionMap()
    {
        if (playerInput == null || playerInput.actions == null)
            return;

        if (!string.IsNullOrEmpty(savedActionMapName) && playerInput.actions.FindActionMap(savedActionMapName, true) != null)
        {
            playerInput.SwitchCurrentActionMap(savedActionMapName);
            return;
        }

        if (playerInput.actions.FindActionMap("Player", true) != null)
            playerInput.SwitchCurrentActionMap("Player");
    }

    private void EnsurePlayerActionMap()
    {
        if (playerInput == null || playerInput.actions == null)
            return;

        if (playerInput.currentActionMap != null && playerInput.currentActionMap.name == "Player")
            return;

        if (playerInput.actions.FindActionMap("Player", true) != null)
            playerInput.SwitchCurrentActionMap("Player");
    }

    private void LockPlayer()
    {
        ResolvePlayerRefs();

        SaveCurrentActionMap();

        if (playerInput != null)
            playerInput.DeactivateInput();

        if (playerController != null)
            playerController.enabled = false;

        if (playerCombat != null)
            playerCombat.enabled = false;

        FreezePhysics();
    }

    public void UnlockPlayer()
    {
        ResolvePlayerRefs();

        RestorePhysics();

        if (playerController != null)
            playerController.enabled = true;

        if (playerCombat != null)
            playerCombat.enabled = true;

        if (playerInput != null)
        {
            playerInput.enabled = true;
            RestoreActionMap();
            EnsurePlayerActionMap();
            playerInput.ActivateInput();
        }
        
        StartCoroutine(DelayedInputReset());
    }

    private IEnumerator DelayedInputReset()
    {
        yield return null;
        yield return null;

        ResolvePlayerRefs();

        if (playerInput != null)
        {
            playerInput.enabled = false;
            playerInput.enabled = true;
        }
    }

    public void FinalizeSceneLoad()
    {
        ResolvePlayerRefs();

        if (SpawnStart != null)
            PlacePlayerAtStart();

        if (!deathSequenceActive && !(playIntroFlyover && levelFlyover != null))
            UnlockPlayer();
    }

    private IEnumerator Initialize()
    {
        float waitTimer = 0f;
        const float managerWaitTimeout = 2f;

        while (AudioManager.Instance == null && waitTimer < managerWaitTimeout)
        {
            waitTimer += Time.unscaledDeltaTime;
            yield return null;
        }

        dialogueManager = DialogueManager.Instance;

        ResolvePlayerRefs();
        yield return null;
        ResolvePlayerRefs();

        if (SpawnStart != null)
            PlacePlayerAtStart();

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetRealm(currentRealm);

            if (playLevelMusic)
            {
                if (hasIntroMusic)
                    AudioManager.Instance.PlayIntroThenLoop(introMusic, loopMusic);
                else
                    AudioManager.Instance.PlayIntroThenLoop(loopMusic, loopMusic);
            }
        }

        StartCoroutine(LevelIntroSequence());
    }

    private void PlacePlayerAtStart()
    {
        ResolvePlayerRefs();

        if (SpawnStart == null || playerRoot == null)
            return;

        if (playerRigidbody != null)
        {
            playerRigidbody.position = SpawnStart.position;
            playerRigidbody.rotation = SpawnStart.rotation;
            playerRigidbody.velocity = Vector3.zero;
            playerRigidbody.angularVelocity = Vector3.zero;
        }

        playerRoot.transform.position = SpawnStart.position;
        playerRoot.transform.rotation = SpawnStart.rotation;
    }

    public static void TriggerPlayerDeath()
    {
        try { OnPlayerDeathEvent?.Invoke(); }
        catch (Exception) { }

        if (Instance != null)
            Instance.BeginDeathSequence();
    }

    private void BeginDeathSequence()
    {
        if (deathSequenceActive)
            return;

        deathSequenceActive = true;

        LockPlayer();
        StartCoroutine(HandlePlayerDeath());
    }

    private IEnumerator HandlePlayerDeath()
    {
        if (DialogueManager.Instance != null)
            DialogueManager.Instance.ResetDialogue();

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayDeathTheme(0.15f, 0.15f);
            if (deathSound != null)
                AudioManager.Instance.PlaySFX(deathSound.clip);
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

        Time.timeScale = 0f;
        yield break;
    }

    public void RespawnFromDeathScreen()
    {
        StartCoroutine(RespawnFlow());
    }

    private IEnumerator RespawnFlow()
    {
        Time.timeScale = 1f;

        if (DeathScreenUI.Instance != null)
            DeathScreenUI.Instance.Hide();

        if (fadeCanvas != null)
            fadeCanvas.alpha = 0f;

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.ResetDeathThemeState();
            AudioManager.Instance.SetRealm(currentRealm);

            if (playLevelMusic)
                AudioManager.Instance.PlayIntroThenLoop(loopMusic, loopMusic);
            else
                AudioManager.Instance.FadeOutMusic(0.05f);
        }

        if (deathSound != null)
            deathSound.Stop();

        LockPlayer();

        if (PlayerRespawn.Instance != null)
            PlayerRespawn.Instance.RespawnPlayer();

        while (PlayerRespawn.Instance != null && PlayerRespawn.Instance.IsRespawning)
            yield return null;

        deathSequenceActive = false;

        ResolvePlayerRefs();
        yield return null;

        if (playerController != null)
            playerController.Revive();

        UnlockPlayer();
    }

    private IEnumerator LevelIntroSequence()
    {
        ResolvePlayerRefs();

        bool shouldPlayFlyover = playIntroFlyover && levelFlyover != null;
        bool shouldShowText = showIntroText && !string.IsNullOrWhiteSpace(introText) && introTextSeconds > 0f && dialogueManager != null;

        if (!shouldPlayFlyover && !shouldShowText)
        {
            UnlockPlayer();
            yield break;
        }

        LockPlayer();

        bool flyoverDone = !shouldPlayFlyover;
        bool textDone = !shouldShowText;

        if (shouldPlayFlyover)
            levelFlyover.Play(() => flyoverDone = true);

        if (shouldShowText)
        {
            yield return dialogueManager.InstructionalText(introText, introTextSeconds);
            textDone = true;
        }

        while (!flyoverDone || !textDone)
            yield return null;

        if (!deathSequenceActive)
            UnlockPlayer();
    }

    public void CompleteLevel()
    {
        if (deathSequenceActive || levelCompleteSequenceActive)
            return;

        levelCompleteSequenceActive = true;

        int nextIndex = SceneManager.GetActiveScene().buildIndex + 1;

        LockPlayer();
        StartCoroutine(FadeThenLoadNext(nextIndex));
    }

    private IEnumerator FadeThenLoadNext(int targetBuildIndex)
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.FadeOutMusic(levelCompleteFadeOutSeconds);

        float fadeSeconds = Mathf.Max(0f, levelCompleteFadeToBlackSeconds);

        if (fadeCanvas != null)
        {
            float startAlpha = fadeCanvas.alpha;

            if (fadeSeconds <= 0f)
            {
                fadeCanvas.alpha = 1f;
            }
            else
            {
                float t = 0f;
                while (t < fadeSeconds)
                {
                    t += Time.unscaledDeltaTime;
                    float u = Mathf.Clamp01(t / fadeSeconds);
                    fadeCanvas.alpha = Mathf.Lerp(startAlpha, 1f, u);
                    yield return null;
                }

                fadeCanvas.alpha = 1f;
            }
        }
        else
        {
            if (fadeSeconds > 0f)
                yield return new WaitForSecondsRealtime(fadeSeconds);
        }

        if (AsyncLoader.Instance == null)
        {
            levelCompleteSequenceActive = false;
            yield break;
        }

        AsyncLoader.Instance.LoadScene(targetBuildIndex, nextRealm, true);
    }
}