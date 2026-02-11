// AsyncLoader.cs
using System.Collections;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// AsyncLoader = swaps from main menu into an additive-loaded gameplay scene with a loading screen.
/// - Shows loading screen immediately, starts a music fade-out, and guarantees the loading screen stays visible
///   for at least minLoadingScreenTime (real time) so fades/UX have time to complete even if the scene loads very fast.
/// - Disables menu cameras/lights/volumes before allowing scene activation to avoid carryover.
/// How to use:
/// - Assign mainMenu, loadingScreen, and loadingSlider.
/// - Adjust musicFadeOutTime and minLoadingScreenTime in Inspector.
/// - Call LoadLevelBtn(sceneName) from UI.
/// </summary>
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
        // Record when loading screen became visible (real time)
        float loadingScreenShownAt = Time.realtimeSinceStartup;

        AsyncOperation loadOperation = SceneManager.LoadSceneAsync(levelToLoad, LoadSceneMode.Additive);
        if (loadOperation == null) yield break;

        loadOperation.allowSceneActivation = false;

        // Start music fade-out immediately (unscaled/unaffected by Time.timeScale)
        if (AudioManager.Instance != null)
            AudioManager.Instance.FadeOutMusic(musicFadeOutTime);

        // Wait for Unity loading progress to reach the "ready to activate" threshold
        while (loadOperation.progress < 0.9f)
        {
            loadingSlider.value = Mathf.Clamp01(loadOperation.progress / 0.9f);
            yield return null;
        }

        loadingSlider.value = 1f;

        // Calculate how long we've already shown the loading screen (realtime)
        float elapsed = Time.realtimeSinceStartup - loadingScreenShownAt;

        // Ensure the loading screen remains visible for at least the minimum time
        float remaining = minLoadingScreenTime - elapsed;
        if (remaining > 0f)
            yield return new WaitForSecondsRealtime(remaining);

        // At this point we've shown the loading screen for long enough; disable menu influencers.
        DisableMenuSceneInfluencers();

        // Allow the scene to activate now that the UX buffer has completed.
        loadOperation.allowSceneActivation = true;

        // Wait until the scene is actually loaded and valid
        Scene loadedScene = default;
        while (true)
        {
            loadedScene = SceneManager.GetSceneByName(levelToLoad);
            if (loadedScene.IsValid() && loadedScene.isLoaded) break;
            yield return null;
        }

        SceneManager.SetActiveScene(loadedScene);

        // Optionally: request level music fade-in here (example)
        // AudioManager.Instance.SetRealm(RealmType.CreteValley);
        // AudioManager.Instance.CrossfadeTo(MusicState.LoopA, 0.8f);

        // Finally unload menu scene
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