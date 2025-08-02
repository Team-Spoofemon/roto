using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public GameObject player;
    public Vector3 offset = new Vector3(0, 2, -6); // Offset behind and above the player
    public float rotationDamping = 5f;             // How quickly camera rotates to match player

    void LateUpdate()
    {
        if (!player) return;

        // Desired rotation to match player's Y rotation
        float desiredYAngle = player.transform.eulerAngles.y;
        float currentYAngle = transform.eulerAngles.y;

        // Smoothly interpolate Y rotation
        float smoothYAngle = Mathf.LerpAngle(currentYAngle, desiredYAngle, rotationDamping * Time.deltaTime);
        Quaternion rotation = Quaternion.Euler(0, smoothYAngle, 0);

        // Calculate new position based on the offset and rotation
        Vector3 desiredPosition = player.transform.position + rotation * offset;

        transform.position = desiredPosition;
        transform.LookAt(player.transform); // Always look at the player
    }
}
