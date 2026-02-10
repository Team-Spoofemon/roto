using UnityEngine;
using UnityEngine.SceneManagement;

public class CoreFix : MonoBehaviour
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
        if (!CoreStart.HasScene()) return;

        if (waitTime > 0f)
            Invoke(nameof(LoadScene), waitTime);
        else
            LoadScene();
    }

    private void LoadScene()
    {
        int index = CoreStart.GetSceneIndex();
        CoreStart.ClearData();

        if (index >= 0 && index != 0)
            SceneManager.LoadScene(index);
    }
}
