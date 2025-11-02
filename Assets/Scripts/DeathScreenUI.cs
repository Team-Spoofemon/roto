using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class DeathScreenUI : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TMPro.TextMeshProUGUI youDiedText;
    [SerializeField] private TMPro.TextMeshProUGUI respawnText;
    [SerializeField] private float fadeDuration = 1f;
    [SerializeField] private float delayBeforeRespawnText = 2f;

    private bool canRespawn = false;
    private bool isActive = false;

    private void Awake()
    {
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();

        canvasGroup.alpha = 0;
        youDiedText.gameObject.SetActive(false);
        respawnText.gameObject.SetActive(false);
    }

    public void Show()
    {
        gameObject.SetActive(true);
        StartCoroutine(ShowSequence());
    }

    private IEnumerator ShowSequence()
    {
        isActive = true;
        canvasGroup.alpha = 0;
        yield return FadeCanvas(1f);

        youDiedText.gameObject.SetActive(true);
        yield return new WaitForSeconds(delayBeforeRespawnText);
        respawnText.gameObject.SetActive(true);
        canRespawn = true;
    }

    private IEnumerator FadeCanvas(float target)
    {
        float start = canvasGroup.alpha;
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(start, target, t / fadeDuration);
            yield return null;
        }
        canvasGroup.alpha = target;
    }

    private void Update()
    {
        if (!isActive || !canRespawn)
            return;

        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            StartCoroutine(HideAndRespawn());
        }
    }

    private IEnumerator HideAndRespawn()
    {
        canRespawn = false;
        yield return FadeCanvas(0f);
        gameObject.SetActive(false);
        if (PlayerRespawn.Instance != null)
            PlayerRespawn.Instance.RespawnPlayer();
    }
}
