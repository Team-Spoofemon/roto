using UnityEngine;

public class CameraFlyoverPath : MonoBehaviour
{
    [Header("Path Settings")]
    public Transform[] waypoints;
    public float moveSpeed = 5f;
    public float rotationSpeed = 2f;
    public float arrivalThreshold = 0.05f;

    [Header("Ease In-Out")]
    public AnimationCurve speedCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Behavior")]
    public bool playOnStart = true;
    public bool loop = false;

    private int currentIndex = 0;
    private Vector3 startPos;
    private Quaternion startRot;
    private float journeyLength;
    private float startTime;
    private bool isFlying = false;

    void Start()
    {
        if (waypoints.Length == 0)
        {
            Debug.LogWarning("CameraFlyoverPath: No waypoints assigned!");
            return;
        }

        if (playOnStart) StartFlyover();
    }

    public void StartFlyover()
    {
        transform.position = waypoints[0].position;
        transform.rotation = waypoints[0].rotation;
        currentIndex = 1;
        BeginNextSegment();
    }

    void BeginNextSegment()
    {
        if (currentIndex >= waypoints.Length)
        {
            if (loop)
            {
                currentIndex = 0;
                BeginNextSegment();
            }
            else
            {
                isFlying = false;
                return;
            }
        }

        startPos = transform.position;
        startRot = transform.rotation;
        journeyLength = Vector3.Distance(startPos, waypoints[currentIndex].position);
        startTime = Time.time;
        isFlying = true;
    }

    void Update()
    {
        if (!isFlying || waypoints.Length == 0) return;

        Transform target = waypoints[currentIndex];
        float distCovered = (Time.time - startTime) * moveSpeed;
        float fracJourney = Mathf.Clamp01(distCovered / journeyLength);
        float easedT = speedCurve.Evaluate(fracJourney);

        transform.position = Vector3.Lerp(startPos, target.position, easedT);
        transform.rotation = Quaternion.Slerp(startRot, target.rotation, easedT * rotationSpeed);

        if (Vector3.Distance(transform.position, target.position) < arrivalThreshold)
        {
            currentIndex++;
            BeginNextSegment();
        }
    }
}
