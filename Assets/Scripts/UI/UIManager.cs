using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;
using TMPro;

public enum DialogueType
{
    Character,
    Instruction,
    Narration
}

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [SerializeField] private VisualElement healthBar;
    [SerializeField] private VisualElement mainMenu;
    [SerializeField] private VisualElement inGameUI;
    [SerializeField] private VisualElement loadingScreen;

    public GameObject dialoguePanel;
    public TextMeshProUGUI dialogueText;
    public GameObject nameBox;
    public TextMeshProUGUI nameText;
    public GameObject cinematicOverlay;
    public TextMeshProUGUI narrationText;
    public CanvasGroup dialogueCanvasGroup;

    [Header("Hover Prompt")]
    [SerializeField] private GameObject promptPanel;
    [SerializeField] private TextMeshProUGUI promptText;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        HidePrompt();
    }

    public void BindUIDocument(UIDocument doc)
    {
        var root = doc.rootVisualElement;
        healthBar = root.Q<VisualElement>("HealthBar");
        mainMenu = root.Q<VisualElement>("MainMenu");
        inGameUI = root.Q<VisualElement>("InGameUI");
        loadingScreen = root.Q<VisualElement>("LoadingScreen");
    }

    public void UpdateHealth(float value)
    {
        if (healthBar == null) return;
        ProgressBar bar = healthBar.Q<ProgressBar>();
        if (bar != null) bar.value = value;
    }

    public void ShowPrompt(string text)
    {
        if (promptPanel == null || promptText == null) return;
        promptText.text = text;
        promptPanel.SetActive(true);
    }

    public void HidePrompt()
    {
        if (promptPanel == null || promptText == null) return;
        promptText.text = "";
        promptPanel.SetActive(false);
    }

    public IEnumerator ShowDialogueLine(string name, string text, float typeSpeed, DialogueType type, float autoHideTime)
    {
        dialoguePanel.SetActive(true);
        dialogueCanvasGroup.alpha = 0f;
        dialogueText.text = "";
        dialogueText.gameObject.SetActive(true);

        if (!string.IsNullOrEmpty(name))
        {
            nameBox.SetActive(true);
            nameText.text = name;
        }
        else
        {
            nameBox.SetActive(false);
        }

        yield return null;

        float t = 0f;
        float fadeInDuration = 0.2f;

        while (t < fadeInDuration)
        {
            t += Time.deltaTime;
            dialogueCanvasGroup.alpha = Mathf.Lerp(0f, 1f, t / fadeInDuration);
            yield return null;
        }

        dialogueCanvasGroup.alpha = 1f;

        bool allowSkip = type == DialogueType.Character;

        foreach (char c in text)
        {
            if (allowSkip && Input.GetMouseButtonDown(0))
            {
                dialogueText.text = text;
                break;
            }
            dialogueText.text += c;
            yield return new WaitForSeconds(typeSpeed);
        }

        if (type == DialogueType.Character)
        {
            bool clicked = false;
            while (!clicked)
            {
                if (Input.GetMouseButtonDown(0))
                    clicked = true;
                yield return null;
            }
        }
        else
        {
            yield return new WaitForSeconds(autoHideTime);
        }

        float fadeOutDuration = 0.2f;
        t = 0f;

        while (t < fadeOutDuration)
        {
            t += Time.deltaTime;
            dialogueCanvasGroup.alpha = Mathf.Lerp(1f, 0f, t / fadeOutDuration);
            yield return null;
        }

        dialogueCanvasGroup.alpha = 0f;
        dialoguePanel.SetActive(false);
        nameBox.SetActive(false);
        dialogueText.text = "";
    }

    public void DisplayNarration(string text)
    {
        cinematicOverlay.SetActive(true);
        narrationText.text = text;
    }

    public void HideNarration()
    {
        cinematicOverlay.SetActive(false);
        narrationText.text = "";
    }

    public void HideTextbox()
    {
        dialoguePanel.SetActive(false);
        dialogueText.text = "";
        nameBox.SetActive(false);
        dialogueCanvasGroup.alpha = 0f;
    }

    public void ResetDialogueUI()
    {
        StopAllCoroutines();
        dialoguePanel.SetActive(false);
        nameBox.SetActive(false);
        dialogueCanvasGroup.alpha = 0f;
        dialogueText.text = "";
        if (cinematicOverlay != null) cinematicOverlay.SetActive(false);
        if (narrationText != null) narrationText.text = "";
        HidePrompt();
    }
}