using UnityEngine;

public class EndLevelTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (LevelManager.Instance != null)
            LevelManager.Instance.CompleteLevel();
    }
}