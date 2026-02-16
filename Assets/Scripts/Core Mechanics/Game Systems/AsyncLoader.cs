
using System.Collections;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class AsyncLoader : MonoBehaviour
{
    [SerializeField] private GameObject loadingScreen;
    [SerializeField] private GameObject mainMenu;
    [SerializeField] private Slider loadingSlider;

    [Header("Timing (seconds, real-time)")]
    [SerializeField, Tooltip("How long to fade out menu music when loading begins.")] private float musicFadeOutTime = 1.5f;
    [SerializeField, Tooltip("Minimum time the loading screen must remain visible (real time).")] private float minLoadingScreenTime = 2.5f;

    private const string MainMenuSceneName = "0B. Main Menu";

    public void LoadLevelBtn(string levelToLoad)
    {
        mainMenu.SetActive(false);
        loadingScreen.SetActive(true);
        StartCoroutine(LoadLevelAsync(levelToLoad));
    }

    IEnumerator LoadLevelAsync(string levelToLoad)
    {
        float loadingScreenShownAt = Time.realtimeSinceStartup;

        AsyncOperation loadOperation = SceneManager.LoadSceneAsync(levelToLoad, LoadSceneMode.Additive);
        if (loadOperation == null) yield break;

        loadOperation.allowSceneActivation = false;

        if (AudioManager.Instance != null)
            AudioManager.Instance.FadeOutMusic(musicFadeOutTime);

        while (loadOperation.progress < 0.9f)
        {
            loadingSlider.value = Mathf.Clamp01(loadOperation.progress / 0.9f);
            yield return null;
        }

        loadingSlider.value = 1f;

        float elapsed = Time.realtimeSinceStartup - loadingScreenShownAt;

        float remaining = minLoadingScreenTime - elapsed;
        if (remaining > 0f)
            yield return new WaitForSecondsRealtime(remaining);

        DisableMenuSceneInfluencers();
        loadOperation.allowSceneActivation = true;

        Scene loadedScene = default;
        while (true)
        {
            loadedScene = SceneManager.GetSceneByName(levelToLoad);
            if (loadedScene.IsValid() && loadedScene.isLoaded) break;
            yield return null;
        }

        SceneManager.SetActiveScene(loadedScene);

        SceneManager.UnloadSceneAsync(MainMenuSceneName);
    }

    private void DisableMenuSceneInfluencers()
    {
        Scene menuScene = SceneManager.GetSceneByName(MainMenuSceneName);
        if (!menuScene.IsValid() || !menuScene.isLoaded) return;

        GameObject[] roots = menuScene.GetRootGameObjects();

        for (int i = 0; i < roots.Length; i++)
        {
            Camera[] cameras = roots[i].GetComponentsInChildren<Camera>(true);
            for (int c = 0; c < cameras.Length; c++)
                cameras[c].enabled = false;

            Light[] lights = roots[i].GetComponentsInChildren<Light>(true);
            for (int l = 0; l < lights.Length; l++)
            {
                if (lights[l].type == LightType.Directional)
                    lights[l].enabled = false;
            }

            DisableVolumesViaReflection(roots[i]);
        }
    }

    private void DisableVolumesViaReflection(GameObject root)
    {
        System.Type volumeType =
            System.Type.GetType("UnityEngine.Rendering.Volume, Unity.RenderPipelines.Core.Runtime") ??
            System.Type.GetType("UnityEngine.Rendering.Volume, Unity.RenderPipelines.Core") ??
            System.Type.GetType("UnityEngine.Rendering.Volume, UnityEngine");

        if (volumeType == null) return;

        Component[] volumes = root.GetComponentsInChildren(volumeType, true);
        for (int i = 0; i < volumes.Length; i++)
        {
            var enabledProp = volumeType.GetProperty("enabled", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
            if (enabledProp != null && enabledProp.PropertyType == typeof(bool) && enabledProp.CanWrite)
                enabledProp.SetValue(volumes[i], false);
        }
    }
}