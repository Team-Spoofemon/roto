using UnityEngine;
using System.Collections;

public class CameraFlyoverPath : MonoBehaviour
{
    [Header("Camera Path")]
    public Transform[] waypoints;
    public float moveSpeed = 2f;
    public float rotationSpeed = 1f;
    public float pauseDuration = 2f;
    [Range(0.001f, 0.5f)] public float arrivalThreshold = 0.05f;

    [Header("Ease In Out")]
    public AnimationCurve speedCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Behavior")]
    public bool playOnStart = true;
    public bool loop = false;

    [Header("Storybook Animation")]
    public Animator storybookAnimator;
    public string openBookTrigger = "OpenBook";

    private int currentIndex = 1;
    private float t = 0f;
    private bool isFlying = false;
    private bool hasTriggeredBook = false;

    void Start()
    {
        if (waypoints == null || waypoints.Length < 2)
        {
            Debug.LogWarning("Ensure there are at least two waypoints assigned bro.");
            return;
        }

        if (playOnStart)
            StartFlyover();
    }

    public void StartFlyover()
    {
        currentIndex = 1;
        t = 0f;
        isFlying = true;
        hasTriggeredBook = false;
        transform.position = waypoints[0].position;
        transform.rotation = waypoints[0].rotation;
    }

    void Update()
    {
        if (!isFlying || waypoints.Length < 2) return;

        t += Time.deltaTime * moveSpeed /
             Vector3.Distance(waypoints[currentIndex - 1].position, waypoints[currentIndex].position);

        float easedT = speedCurve.Evaluate(Mathf.Clamp01(t));

        Vector3 newPos = GetSplinePosition(currentIndex, easedT);
        Quaternion newRot = Quaternion.Slerp(
            waypoints[currentIndex - 1].rotation,
            waypoints[currentIndex].rotation,
            easedT * rotationSpeed);

        transform.position = newPos;
        transform.rotation = newRot;

        if (!hasTriggeredBook && currentIndex == waypoints.Length - 1 && t >= 0.9f && storybookAnimator != null)
        {
            storybookAnimator.SetTrigger(openBookTrigger);
            hasTriggeredBook = true;
        }

        if (t >= 1f)
        {
            currentIndex++;
            moveSpeed *= 0.9f;
            t = 0f;

            if (currentIndex >= waypoints.Length)
            {
                if (loop)
                    currentIndex = 1;
                else
                {
                    isFlying = false;
                    return;
                }
            }
        }
    }

    Vector3 GetSplinePosition(int i, float t)
    {
        int i0 = Mathf.Clamp(i - 2, 0, waypoints.Length - 1);
        int i1 = Mathf.Clamp(i - 1, 0, waypoints.Length - 1);
        int i2 = Mathf.Clamp(i, 0, waypoints.Length - 1);
        int i3 = Mathf.Clamp(i + 1, 0, waypoints.Length - 1);

        Vector3 p0 = waypoints[i0].position;
        Vector3 p1 = waypoints[i1].position;
        Vector3 p2 = waypoints[i2].position;
        Vector3 p3 = waypoints[i3].position;

        return 0.5f * (
            (2f * p1) +
            (-p0 + p2) * t +
            (2f * p0 - 5f * p1 + 4f * p2 - p3) * (t * t) +
            (-p0 + 3f * p1 - 3f * p2 + p3) * (t * t * t)
        );
    }
}
