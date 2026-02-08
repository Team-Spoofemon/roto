// MainMenu.cs
using UnityEngine;

/// <summary>
/// MainMenu = controls main menu UI flow.
/// Music UX:
/// - Starts menu music (AudioProfile.mainTheme) through AudioManager when the menu becomes active.
/// - Loading transition fades music out via AsyncLoader.
/// How to use:
/// - Call ShowMenu() when menu should be visible.
/// - Ensure AudioManager exists in Core/Bootstrap and has an AudioProfile assigned.
/// </summary>
public class MainMenu : MonoBehaviour
{
    [SerializeField] private float menuMusicFadeInTime = 0.35f;

    private void Start()
    {
        StartMenuMusic();
    }

    public void StartMenuMusic()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayMainTheme(menuMusicFadeInTime);
    }
}