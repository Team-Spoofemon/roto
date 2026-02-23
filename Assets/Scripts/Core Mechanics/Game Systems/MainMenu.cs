using UnityEngine;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private float menuMusicFadeInTime = 0f;

    [Header("Start Game Target")]
    [SerializeField] private string firstLevelSceneName = "1A. CM 1";
    [SerializeField] private string creditsSceneName = "8. End Credits";

    [Header("Realms")]
    [SerializeField] private RealmType firstLevelRealm = RealmType.CreteValley;
    [SerializeField] private RealmType creditsRealm = RealmType.cutsceneRealm;

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

        AsyncLoader.Instance.LoadScene(firstLevelSceneName, firstLevelRealm, true);
    }

    public void StartCredits()
    {
        if (AsyncLoader.Instance == null)
            return;

        AsyncLoader.Instance.LoadScene(creditsSceneName, creditsRealm, true);
    }
}