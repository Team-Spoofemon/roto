using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class AsyncLoader : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject loadingScreen;
    [SerializeField] private Slider loadingSlider;

    [Header("Timing")]
    [SerializeField] private float musicFadeOutTime = 1.5f;
    [SerializeField] private float minLoadingScreenTime = 2.5f;

    [Header("Scenes")]
    [SerializeField] private string coreSceneName = "0A. Core";
    [SerializeField] private string mainMenuSceneName = "0B. Main Menu";

    public static AsyncLoader Instance;

    private bool isLoading;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (loadingScreen != null)
            loadingScreen.SetActive(false);

        if (loadingSlider != null)
        {
            loadingSlider.minValue = 0f;
            loadingSlider.maxValue = 1f;
            loadingSlider.wholeNumbers = false;
            loadingSlider.value = 0f;
        }
    }

    public void LoadScene(string sceneName, RealmType nextRealm, bool fadeMusicFirst = true)
    {
        if (isLoading) return;
        if (string.IsNullOrEmpty(sceneName)) return;
        StartCoroutine(LoadSceneRoutine(sceneName, -1, nextRealm, fadeMusicFirst));
    }

    public void LoadScene(int buildIndex, RealmType nextRealm, bool fadeMusicFirst = true)
    {
        if (isLoading) return;
        if (buildIndex < 0) return;
        StartCoroutine(LoadSceneRoutine(null, buildIndex, nextRealm, fadeMusicFirst));
    }

    private IEnumerator LoadSceneRoutine(string sceneName, int buildIndex, RealmType nextRealm, bool fadeMusicFirst)
    {
        isLoading = true;

        Scene oldActive = SceneManager.GetActiveScene();

        if (loadingScreen != null)
            loadingScreen.SetActive(false);

        if (loadingSlider != null)
            loadingSlider.value = 0f;

        yield return null;

        if (fadeMusicFirst && AudioManager.Instance != null)
        {
            if (musicFadeOutTime > 0f)
                yield return AudioManager.Instance.FadeOutCoroutine(musicFadeOutTime);
            else
                AudioManager.Instance.FadeOutMusic(0f);
        }

        if (loadingScreen != null)
            loadingScreen.SetActive(true);

        if (loadingSlider != null)
            loadingSlider.value = 0f;

        yield return null;

        float shownAt = Time.realtimeSinceStartup;

        AsyncOperation op;
        if (!string.IsNullOrEmpty(sceneName))
            op = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        else
            op = SceneManager.LoadSceneAsync(buildIndex, LoadSceneMode.Additive);

        if (op == null)
        {
            if (loadingScreen != null)
                loadingScreen.SetActive(false);

            isLoading = false;
            yield break;
        }

        op.allowSceneActivation = false;

        while (op.progress < 0.9f)
        {
            float p = Mathf.Clamp01(op.progress / 0.9f);
            if (loadingSlider != null)
                loadingSlider.value = p;

            yield return null;
        }

        float elapsed = Time.realtimeSinceStartup - shownAt;
        float remaining = minLoadingScreenTime - elapsed;
        if (remaining > 0f)
            yield return new WaitForSecondsRealtime(remaining);

        if (loadingSlider != null)
            loadingSlider.value = 1f;

        op.allowSceneActivation = true;

        Scene newScene;
        while (true)
        {
            if (!string.IsNullOrEmpty(sceneName))
                newScene = SceneManager.GetSceneByName(sceneName);
            else
                newScene = SceneManager.GetSceneByBuildIndex(buildIndex);

            if (newScene.IsValid() && newScene.isLoaded)
                break;

            yield return null;
        }

        SceneManager.SetActiveScene(newScene);

        if (AudioManager.Instance != null)
            AudioManager.Instance.SetRealm(nextRealm);

        if (oldActive.IsValid() && oldActive.isLoaded && oldActive != newScene && oldActive.name != coreSceneName)
            SceneManager.UnloadSceneAsync(oldActive);

        Scene menu = SceneManager.GetSceneByName(mainMenuSceneName);
        if (menu.IsValid() && menu.isLoaded && menu != newScene)
            SceneManager.UnloadSceneAsync(menu);

        if (loadingScreen != null)
            loadingScreen.SetActive(false);

        isLoading = false;
    }
}