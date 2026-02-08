using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class DeathScreenUI : MonoBehaviour
{
    public static DeathScreenUI Instance { get; private set; }

    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TMPro.TextMeshProUGUI youDiedText;
    [SerializeField] private TMPro.TextMeshProUGUI respawnText;
    [SerializeField] private float fadeDuration = 1f;
    [SerializeField] private float delayBeforeRespawnText = 2f;

    private bool canRespawn;
    private bool isActive;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();

        canvasGroup.alpha = 0f;

        if (youDiedText) youDiedText.gameObject.SetActive(false);
        if (respawnText) respawnText.gameObject.SetActive(false);

        canRespawn = false;
        isActive = false;
    }

    public void Show()
    {
        StopAllCoroutines();
        gameObject.SetActive(true);
        StartCoroutine(ShowSequence());
    }

    public void Hide()
    {
        StopAllCoroutines();
        isActive = false;
        canRespawn = false;

        if (canvasGroup) canvasGroup.alpha = 0f;
        if (youDiedText) youDiedText.gameObject.SetActive(false);
        if (respawnText) respawnText.gameObject.SetActive(false);

        gameObject.SetActive(false);
    }

    private IEnumerator ShowSequence()
    {
        isActive = true;
        canRespawn = false;

        if (youDiedText) youDiedText.gameObject.SetActive(false);
        if (respawnText) respawnText.gameObject.SetActive(false);

        if (canvasGroup) canvasGroup.alpha = 0f;
        yield return FadeCanvas(1f);

        if (youDiedText) youDiedText.gameObject.SetActive(true);

        yield return new WaitForSecondsRealtime(delayBeforeRespawnText);

        if (respawnText) respawnText.gameObject.SetActive(true);
        canRespawn = true;
    }

    private IEnumerator FadeCanvas(float target)
    {
        if (!canvasGroup) yield break;

        float start = canvasGroup.alpha;
        float t = 0f;

        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Lerp(start, target, t / fadeDuration);
            yield return null;
        }

        canvasGroup.alpha = target;
    }

    private void Update()
    {
        if (!isActive || !canRespawn)
            return;

        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
            StartCoroutine(HideAndRespawn());
    }

    private IEnumerator HideAndRespawn()
    {
        canRespawn = false;

        yield return FadeCanvas(0f);

        Hide();

        if (LevelManager.Instance != null)
            LevelManager.Instance.RespawnFromDeathScreen();
        else if (PlayerRespawn.Instance != null)
        {
            Time.timeScale = 1f;
            PlayerRespawn.Instance.RespawnPlayer();
        }
    }
}