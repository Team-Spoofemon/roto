using UnityEngine;

public class GameObjectTrigger : MonoBehaviour
{
    [SerializeField] private GameObject firstCamObj;
    [SerializeField] private GameObject secondCamObj;
    [SerializeField] private bool switchToSecondCamera;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (firstCamObj == null || secondCamObj == null) return;

        if (switchToSecondCamera)
        {
            firstCamObj.SetActive(false);
            secondCamObj.SetActive(true);
        }
        else
        {
            firstCamObj.SetActive(true);
            secondCamObj.SetActive(false);
        }
    }
}