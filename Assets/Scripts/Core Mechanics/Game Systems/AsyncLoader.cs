using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class AsyncLoader : MonoBehaviour
{
    [SerializeField] private GameObject loadingScreen;
    [SerializeField] private GameObject mainMenu;
    [SerializeField] private Slider loadingSlider;

    public void LoadLevelBtn(string levelToLoad)
    {
        mainMenu.SetActive(false);
        loadingScreen.SetActive(true);
        StartCoroutine(LoadLevelAsync(levelToLoad));
    }

    IEnumerator LoadLevelAsync(string levelToLoad)
    {
        AsyncOperation loadOperation = SceneManager.LoadSceneAsync(levelToLoad, LoadSceneMode.Additive);
        if (loadOperation == null) yield break;

        loadOperation.allowSceneActivation = false;

        if (AudioManager.Instance != null)
            AudioManager.Instance.FadeOutMusic(0.5f);

        while (loadOperation.progress < 0.9f)
        {
            loadingSlider.value = Mathf.Clamp01(loadOperation.progress / 0.9f);
            yield return null;
        }

        loadingSlider.value = 1f;
        yield return new WaitForSeconds(2f);

        loadOperation.allowSceneActivation = true;

        yield return null;

        SceneManager.UnloadSceneAsync("0B. Main Menu");
    }
}
