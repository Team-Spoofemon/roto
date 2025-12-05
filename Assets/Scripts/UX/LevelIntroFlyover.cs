using UnityEngine;
using Cinemachine;
using System.Collections;

public class LevelIntroFlyover : MonoBehaviour
{
    public CinemachineVirtualCamera introCamera;
    public CinemachineDollyCart dollyCart;
    public CinemachineVirtualCamera gameplayCamera;

    public float flyoverDuration = 5f;
    public float landingBlendDuration = 1.5f;

    public PlayerController player;

    private void Start()
    {
        player.SetInputEnabled(false);
        StartCoroutine(FlyoverSequence());
    }

    private IEnumerator FlyoverSequence()
    {
        float t = 0f;
        float pathLength = dollyCart.m_Path.PathLength;

        while (t < flyoverDuration)
        {
            t += Time.deltaTime;
            dollyCart.m_Position = Mathf.Lerp(0f, pathLength, t / flyoverDuration);
            yield return null;
        }

        // Blend to gameplay camera
        introCamera.Priority = 0;
        gameplayCamera.Priority = 20;

        yield return new WaitForSeconds(landingBlendDuration);

        player.SetInputEnabled(true);
    }
}
