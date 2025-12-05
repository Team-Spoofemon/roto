using UnityEngine;
using Cinemachine;
using System.Collections;

public class OrientationShiftZone : MonoBehaviour
{
    public Transform player;
    public CinemachineVirtualCamera virtualCamera;
    public float targetRotation = 90f;
    public float rotateTime = 0.3f;
    public PlayerController playerController;

    bool triggered;

    void OnTriggerEnter(Collider other)
    {
        if (triggered) return;
        if (!other.CompareTag("Player")) return;

        triggered = true;
        StartCoroutine(RotatePlayerAndCamera());
    }

    IEnumerator RotatePlayerAndCamera()
    {
        float elapsed = 0f;

        float startPlayerYaw = player.eulerAngles.y;
        float startCameraYaw = virtualCamera.transform.eulerAngles.y;

        float endPlayerYaw = startPlayerYaw + targetRotation;
        float endCameraYaw = startCameraYaw + targetRotation;

        while (elapsed < rotateTime)
        {
            float t = elapsed / rotateTime;

            float newPlayerYaw = Mathf.LerpAngle(startPlayerYaw, endPlayerYaw, t);
            float newCameraYaw = Mathf.LerpAngle(startCameraYaw, endCameraYaw, t);

            player.rotation = Quaternion.Euler(0, newPlayerYaw, 0);
            virtualCamera.transform.rotation = Quaternion.Euler(0, newCameraYaw, 0);

            elapsed += Time.deltaTime;
            yield return null;
        }

        player.rotation = Quaternion.Euler(0, endPlayerYaw, 0);
        virtualCamera.transform.rotation = Quaternion.Euler(0, endCameraYaw, 0);

        playerController.SetInputRotation(endPlayerYaw);
        playerController.SetSpriteBaseRotation(endPlayerYaw);
    }

}
