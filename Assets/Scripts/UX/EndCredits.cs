using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EndCredits : MonoBehaviour
{
    [Header("Trigger")]
    [SerializeField] private float waitSeconds = 65f;
    [SerializeField] private KeyCode skipKey = KeyCode.Space;

    [Header("Scenes")]
    [SerializeField] private string coreSceneName = "0A. Core";
    [SerializeField] private string mainMenuSceneName = "0B. Main Menu";

    private float timer;
    private bool returning;

    void Update()
    {
        if (returning) return;

        timer += Time.deltaTime;

        if (timer >= waitSeconds || Input.GetKeyDown(skipKey))
        {
            returning = true;
            DontDestroyOnLoad(gameObject);
            StartCoroutine(ReturnToMainMenuRoutine());
        }
    }

    private IEnumerator ReturnToMainMenuRoutine()
    {
        if (!IsSceneLoaded(coreSceneName))
            yield return LoadScene(coreSceneName, LoadSceneMode.Single);

        if (!IsSceneLoaded(mainMenuSceneName))
            yield return LoadScene(mainMenuSceneName, LoadSceneMode.Additive);

        SetActiveScene(mainMenuSceneName);

        yield return UnloadAllExcept(coreSceneName, mainMenuSceneName);

        Destroy(gameObject);
    }

    private static IEnumerator LoadScene(string sceneName, LoadSceneMode mode)
    {
        var op = SceneManager.LoadSceneAsync(sceneName, mode);
        while (!op.isDone) yield return null;
    }

    private static bool IsSceneLoaded(string sceneName)
    {
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            var s = SceneManager.GetSceneAt(i);
            if (s.isLoaded && s.name == sceneName) return true;
        }
        return false;
    }

    private static void SetActiveScene(string sceneName)
    {
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            var s = SceneManager.GetSceneAt(i);
            if (s.isLoaded && s.name == sceneName)
            {
                SceneManager.SetActiveScene(s);
                return;
            }
        }
    }

    private static IEnumerator UnloadAllExcept(string keepSceneA, string keepSceneB)
    {
        for (int i = SceneManager.sceneCount - 1; i >= 0; i--)
        {
            var s = SceneManager.GetSceneAt(i);
            if (!s.isLoaded) continue;
            if (s.name == keepSceneA || s.name == keepSceneB) continue;

            var op = SceneManager.UnloadSceneAsync(s);
            if (op != null)
                while (!op.isDone) yield return null;
        }
    }
}