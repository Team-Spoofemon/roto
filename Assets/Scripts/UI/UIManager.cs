using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements; // For UI Toolkit

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    // Define UI controls, e.g. public fields or refs
    private VisualElement healthBar;
    private VisualElement mainMenu;
    private VisualElement inGameUI;
    private VisualElement loadingScreen;

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

    private void OnEnable() // For UI Toolkit
    {
        var root = GetComponent<UIDocument>().rootVisualElement;
        healthBar = root.Q<VisualElement>("HealthBar");
        mainMenu = root.Q<VisualElement>("MainMenu");
        inGameUI = root.Q<VisualElement>("InGameUI");
        loadingScreen = root.Q<VisualElement>("LoadingScreen");
    }

    // Example: Health Bar Management
    public void UpdateHealth(float value)
    {
        // Assumes HealthBar element contains a ProgressBar or similar
        ProgressBar bar = healthBar.Q<ProgressBar>();
        bar.value = value;
    }

    // Menu Management
    public void ShowMainMenu() => mainMenu.style.display = DisplayStyle.Flex;
    public void HideMainMenu() => mainMenu.style.display = DisplayStyle.None;
    public void ShowInGameUI() => inGameUI.style.display = DisplayStyle.Flex;
    public void HideInGameUI() => inGameUI.style.display = DisplayStyle.None;
    public void ShowLoadingScreen() => loadingScreen.style.display = DisplayStyle.Flex;
    public void HideLoadingScreen() => loadingScreen.style.display = DisplayStyle.None;

    // Additional UI management methods can be added here
}
