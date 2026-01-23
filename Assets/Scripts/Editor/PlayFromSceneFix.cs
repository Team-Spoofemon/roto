#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[InitializeOnLoad]
public static class PlayFromSceneFix
{
    private const string hasSceneKey = "HAS_START_SCENE";
    private const string sceneIndexKey = "START_SCENE_INDEX";

    static PlayFromSceneFix()
    {
        EditorApplication.playModeStateChanged += OnPlayChange;
    }

    private static void OnPlayChange(PlayModeStateChange change)
    {
        if (change == PlayModeStateChange.ExitingEditMode)
        {
            var scene = EditorSceneManager.GetActiveScene();

            PlayerPrefs.SetInt(hasSceneKey, 1);
            PlayerPrefs.SetInt(sceneIndexKey, scene.buildIndex);
            PlayerPrefs.Save();
        }

        if (change == PlayModeStateChange.EnteredEditMode)
        {
            PlayerPrefs.SetInt(hasSceneKey, 0);
            PlayerPrefs.DeleteKey(sceneIndexKey);
            PlayerPrefs.Save();
        }
    }
}
#endif
