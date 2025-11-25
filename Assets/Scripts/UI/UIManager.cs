using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.UI;
using TMPro;

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

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    private void OnEnable()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;
        healthBar = root.Q<VisualElement>("HealthBar");
        mainMenu = root.Q<VisualElement>("MainMenu");
        inGameUI = root.Q<VisualElement>("InGameUI");
        loadingScreen = root.Q<VisualElement>("LoadingScreen");
    }

    public void UpdateHealth(float value)
    {
        ProgressBar bar = healthBar.Q<ProgressBar>();
        bar.value = value;
    }

    public void ShowMainMenu() => mainMenu.style.display = DisplayStyle.Flex;
    public void HideMainMenu() => mainMenu.style.display = DisplayStyle.None;
    public void ShowInGameUI() => inGameUI.style.display = DisplayStyle.Flex;
    public void HideInGameUI() => inGameUI.style.display = DisplayStyle.None;
    public void ShowLoadingScreen() => loadingScreen.style.display = DisplayStyle.Flex;
    public void HideLoadingScreen() => loadingScreen.style.display = DisplayStyle.None;

    public void DisplayTextbox(string text)
    {
        dialogueText.text = text;
        LayoutRebuilder.ForceRebuildLayoutImmediate(dialoguePanel.GetComponent<RectTransform>());
        dialoguePanel.SetActive(true);
        nameBox.SetActive(false);
    }

    public void DisplayNameAndTextbox(string name, string text)
    {
        nameText.text = name;
        dialogueText.text = text;
        LayoutRebuilder.ForceRebuildLayoutImmediate(dialoguePanel.GetComponent<RectTransform>());
        dialoguePanel.SetActive(true);
        nameBox.SetActive(true);
    }

    public void DisplayNarration(string text)
    {
        cinematicOverlay.SetActive(true);
        narrationText.text = text;
    }

    public void SetText(string text)
    {
        dialogueText.text = text;
    }

    public void HideTextbox()
    {
        dialoguePanel.SetActive(false);
        nameBox.SetActive(false);
    }

    public void HideNarration()
    {
        cinematicOverlay.SetActive(false);
    }
}
