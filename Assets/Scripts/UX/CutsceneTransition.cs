using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CutsceneTransition : MonoBehaviour
{
    [Header("Trigger")]
    [SerializeField] private float waitSeconds = 80f;
    [SerializeField] private KeyCode skipKey = KeyCode.Space;

    [Header("Scenes")]
    [SerializeField] private string coreSceneName = "0A. Core";
    [SerializeField] private string sceneToLoad = "1B. Crete Valley";

    [Header("Next Realm")]
    [SerializeField] private RealmType nextRealm = RealmType.CreteValley;

    [Header("Cutscene Music")]
    [SerializeField] private bool autoPlayCutsceneMusic = true;

    private float timer;
    private bool transitioning;

    void Start()
    {
        if (!autoPlayCutsceneMusic) return;

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetRealm(RealmType.cutsceneRealm);
            AudioManager.Instance.SetMusicState(MusicState.Intro);
        }
    }

    void Update()
    {
        if (transitioning) return;

        timer += Time.deltaTime;

        if (timer >= waitSeconds || Input.GetKeyDown(skipKey))
        {
            transitioning = true;
            StartCoroutine(LoadNextRoutine());
        }
    }

    private IEnumerator LoadNextRoutine()
    {
        if (!IsSceneLoaded(coreSceneName))
            yield return LoadScene(coreSceneName, LoadSceneMode.Additive);

        if (AsyncLoader.Instance != null)
        {
            AsyncLoader.Instance.LoadScene(sceneToLoad, nextRealm, true);
            Destroy(gameObject);
            yield break;
        }

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.FadeOutMusic(0.75f);
            yield return new WaitForSecondsRealtime(0.75f);
        }

        yield return LoadScene(sceneToLoad, LoadSceneMode.Additive);
        SetActiveScene(sceneToLoad);

        if (AudioManager.Instance != null)
            AudioManager.Instance.SetRealm(nextRealm);

        yield return UnloadAllExcept(coreSceneName, sceneToLoad);
        Destroy(gameObject);
    }

    private static IEnumerator LoadScene(string sceneName, LoadSceneMode mode)
    {
        var op = SceneManager.LoadSceneAsync(sceneName, mode);
        while (op != null && !op.isDone) yield return null;
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