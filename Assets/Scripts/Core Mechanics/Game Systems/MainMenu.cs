using UnityEngine;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private float menuMusicFadeInTime = 0f;

    [Header("Start Game Target")]
    [SerializeField] private string firstLevelSceneName = "1A. CM 1";
    [SerializeField] private string creditsSceneName = "8. End Credits";

    private void Start()
    {
        StartMenuMusic();
    }

    public void StartMenuMusic()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayMainTheme(menuMusicFadeInTime);
    }

    public void StartGame()
    {
        if (AsyncLoader.Instance == null)
            return;

        else
            AsyncLoader.Instance.LoadScene(firstLevelSceneName);
    }

    public void StartCredits()
    {
        if (AsyncLoader.Instance == null)
            return;

        else
            AsyncLoader.Instance.LoadScene(creditsSceneName);
    }
}