using UnityEngine;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private AsyncLoader loader;

    public void PlayGame()
    {
        loader.LoadLevelBtn("1. Crete Valley");
    }
}
