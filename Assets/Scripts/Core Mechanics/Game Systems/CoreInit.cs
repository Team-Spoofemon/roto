using UnityEngine;
using UnityEngine.SceneManagement;

public class CoreInit : MonoBehaviour
{
    void Start()
    {
        SceneManager.LoadScene("0B. Main Menu", LoadSceneMode.Additive);
    }
}
