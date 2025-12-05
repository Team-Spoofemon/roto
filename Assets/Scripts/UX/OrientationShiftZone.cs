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

        Quaternion startPlayerRot = player.rotation;
        Quaternion startCamRot = virtualCamera.transform.rotation;

        Quaternion endPlayerRot = startPlayerRot * Quaternion.Euler(0, targetRotation, 0);
        Quaternion endCamRot = startCamRot * Quaternion.Euler(0, targetRotation, 0);

        while (elapsed < rotateTime)
        {
            float t = elapsed / rotateTime;
            player.rotation = Quaternion.Slerp(startPlayerRot, endPlayerRot, t);
            virtualCamera.transform.rotation = Quaternion.Slerp(startCamRot, endCamRot, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        player.rotation = endPlayerRot;
        virtualCamera.transform.rotation = endCamRot;

        playerController.SetInputRotation(targetRotation);
        playerController.SetSpriteBaseRotation(targetRotation); 
    }
}
