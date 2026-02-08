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

    private bool triggered;

    private void OnTriggerEnter(Collider other)
    {
        if (triggered) return;
        if (!other.CompareTag("Player")) return;

        triggered = true;

        if (!playerController)
            playerController = other.GetComponent<PlayerController>();

        if (!player)
            player = other.transform;

        if (playerController)
            playerController.SetCameraTransform(virtualCamera.transform);

        StartCoroutine(RotatePlayerAndCamera());
    }

    private IEnumerator RotatePlayerAndCamera()
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

            player.rotation = Quaternion.Euler(0f, newPlayerYaw, 0f);
            virtualCamera.transform.rotation = Quaternion.Euler(0f, newCameraYaw, 0f);

            elapsed += Time.deltaTime;
            yield return null;
        }

        player.rotation = Quaternion.Euler(0f, endPlayerYaw, 0f);
        virtualCamera.transform.rotation = Quaternion.Euler(0f, endCameraYaw, 0f);

        if (playerController)
        {
            playerController.SetInputRotation(endPlayerYaw);
            playerController.SetCameraTransform(virtualCamera.transform);
        }
    }
}