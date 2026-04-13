using UnityEngine;

public class SpriteOrientation : MonoBehaviour
{
    [SerializeField] private bool facingRight = true;
    [SerializeField] private bool allowHorizontalFlip = true;

    private Vector3 originalScale;

    private void Awake()
    {
        originalScale = transform.localScale;
    }

    public void SetFacingRight(bool value)
    {
        facingRight = value;
    }

    private void LateUpdate()
    {
        Camera cam = GetActiveCamera();
        if (cam == null) return;

        Vector3 forward = Vector3.ProjectOnPlane(cam.transform.forward, Vector3.up);
        if (forward.sqrMagnitude <= 0.0001f) return;

        transform.rotation = Quaternion.LookRotation(forward.normalized, Vector3.up);

        Vector3 scale = originalScale;

        if (allowHorizontalFlip)
            scale.x = Mathf.Abs(originalScale.x) * (facingRight ? 1f : -1f);
        else
            scale.x = Mathf.Abs(originalScale.x);

        transform.localScale = scale;
    }

    private Camera GetActiveCamera()
    {
        Camera[] cameras = Camera.allCameras;
        Camera bestCamera = null;
        float highestDepth = float.MinValue;

        for (int i = 0; i < cameras.Length; i++)
        {
            Camera cam = cameras[i];

            if (!cam.isActiveAndEnabled) continue;
            if (cam.cameraType != CameraType.Game) continue;

            if (cam.depth > highestDepth)
            {
                highestDepth = cam.depth;
                bestCamera = cam;
            }
        }

        return bestCamera;
    }
}