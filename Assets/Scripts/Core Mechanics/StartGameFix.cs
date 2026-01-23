using UnityEngine;
using UnityEngine.SceneManagement;

public static class StartGameFix
{
    private const string hasSceneKey = "HAS_START_SCENE";
    private const string sceneIndexKey = "START_SCENE_INDEX";

    public static bool HasScene()
    {
        return PlayerPrefs.GetInt(hasSceneKey, 0) == 1;
    }

    public static int GetSceneIndex()
    {
        return PlayerPrefs.GetInt(sceneIndexKey, -1);
    }

    public static void ClearData()
    {
        PlayerPrefs.SetInt(hasSceneKey, 0);
        PlayerPrefs.DeleteKey(sceneIndexKey);
        PlayerPrefs.Save();
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void OnGameStart()
    {
        if (!HasScene()) return;

        if (SceneManager.GetActiveScene().buildIndex == 0) return;

        SceneManager.LoadScene(0);
    }
}

public class CoreSceneFix : MonoBehaviour
{
    private static bool alreadyHere;

    public float waitTime;

    private void Awake()
    {
        if (alreadyHere)
        {
            Destroy(gameObject);
            return;
        }

        alreadyHere = true;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        if (!StartGameFix.HasScene()) return;

        if (waitTime > 0f)
            Invoke(nameof(LoadScene), waitTime);
        else
            LoadScene();
    }

    private void LoadScene()
    {
        int index = StartGameFix.GetSceneIndex();
        StartGameFix.ClearData();

        if (index >= 0 && index != 0)
            SceneManager.LoadScene(index);
    }
}
