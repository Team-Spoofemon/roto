using System.Collections;
using System.Reflection;
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
    [SerializeField] private float musicFadeInTime = 1.0f;
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

    public void LoadScene(string sceneName)
    {
        if (isLoading) return;
        if (string.IsNullOrEmpty(sceneName)) return;
        StartCoroutine(LoadSceneRoutine(sceneName, -1));
    }

    public void LoadScene(int buildIndex)
    {
        if (isLoading) return;
        if (buildIndex < 0) return;
        StartCoroutine(LoadSceneRoutine(null, buildIndex));
    }

    private IEnumerator LoadSceneRoutine(string sceneName, int buildIndex)
    {
        isLoading = true;

        Scene oldActive = SceneManager.GetActiveScene();

        if (loadingScreen != null)
            loadingScreen.SetActive(true);

        if (loadingSlider != null)
            loadingSlider.value = 0f;

        if (loadingScreen != null)
        loadingScreen.SetActive(true);

        if (loadingSlider != null)
        loadingSlider.value = 0f;

        yield return null;

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.FadeOutMusic(musicFadeOutTime);

            if (musicFadeOutTime > 0f)
                yield return new WaitForSecondsRealtime(musicFadeOutTime);
        }

        float shownAt = Time.realtimeSinceStartup;

        AsyncOperation op;

        if (!string.IsNullOrEmpty(sceneName))
            op = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        else
            op = SceneManager.LoadSceneAsync(buildIndex, LoadSceneMode.Additive);

        if (op == null)
        {
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

        Scene newScene = default;

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

        if (oldActive.IsValid() && oldActive.isLoaded && oldActive != newScene && oldActive.name != coreSceneName)
            SceneManager.UnloadSceneAsync(oldActive);

        Scene menu = SceneManager.GetSceneByName(mainMenuSceneName);
        if (menu.IsValid() && menu.isLoaded && menu != newScene)
            SceneManager.UnloadSceneAsync(menu);

        if (AudioManager.Instance != null)
            TryRestoreMusic(AudioManager.Instance, musicFadeInTime);

        if (loadingScreen != null)
            loadingScreen.SetActive(false);

        isLoading = false;
    }

    private void TryRestoreMusic(object audioManager, float fadeInTime)
    {
        if (audioManager == null) return;

        var type = audioManager.GetType();

        if (TryInvoke(type, audioManager, "FadeInMusic", fadeInTime)) return;
        if (TryInvoke(type, audioManager, "FadeMusicIn", fadeInTime)) return;
        if (TryInvoke(type, audioManager, "RestoreMusic", fadeInTime)) return;
        if (TryInvoke(type, audioManager, "ResetMusicVolume", fadeInTime)) return;
        if (TryInvoke(type, audioManager, "UnmuteMusic", fadeInTime)) return;

        if (TryInvoke(type, audioManager, "FadeInMusic")) return;
        if (TryInvoke(type, audioManager, "FadeMusicIn")) return;
        if (TryInvoke(type, audioManager, "RestoreMusic")) return;
        if (TryInvoke(type, audioManager, "ResetMusicVolume")) return;
        if (TryInvoke(type, audioManager, "UnmuteMusic")) return;

        if (TryInvoke(type, audioManager, "SetMusicVolume", 1f)) return;
        if (TryInvoke(type, audioManager, "SetMusicVolume", 1.0f)) return;
        if (TryInvoke(type, audioManager, "SetMusicVolumeNormalized", 1f)) return;
    }

    private bool TryInvoke(System.Type type, object target, string methodName, float arg)
    {
        var m = type.GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new[] { typeof(float) }, null);
        if (m == null) return false;
        m.Invoke(target, new object[] { arg });
        return true;
    }

    private bool TryInvoke(System.Type type, object target, string methodName)
    {
        var m = type.GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, System.Type.EmptyTypes, null);
        if (m == null) return false;
        m.Invoke(target, null);
        return true;
    }
}