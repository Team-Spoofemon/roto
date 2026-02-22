using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

/// <summary>
/// Level flyover that spline-interpolates an intro camera along waypoint transforms, then hands off to gameplay cleanly.
/// Tail-end fix: the flyover now ends on the *gameplay camera's actual follow/orbit pose* (what CameraFollow will compute),
/// so there is no last-frame orientation pop. The CinemachineBrain is re-enabled on the next frame to avoid a stale-frame pop.
/// </summary>
public class LevelFlyover : MonoBehaviour
{
    [Header("Assign These")]
    [SerializeField] private Camera introCamera;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private CinemachineBrain cinemachineBrain;
    [SerializeField] private CinemachineVirtualCamera vcamStart;
    [SerializeField] private Transform gameplayCameraStart;
    [SerializeField] private Transform waypointsParent;

    [Header("Gameplay Orientation Source")]
    [SerializeField] private CameraFollow gameplayCameraFollow;

    [Header("Tuning")]
    [SerializeField] private float secondsPerSegment = 1.5f;

    [Header("Spline Sampling")]
    [SerializeField] private int samplesPerSegment = 40;

    [Header("Orientation")]
    [SerializeField] private bool useWaypointUp = true;
    [SerializeField] private Vector3 fallbackUp = Vector3.up;

    private bool playing;

    private struct Waypoint
    {
        public Vector3 pos;
        public Quaternion rot;
    }

    private struct Sample
    {
        public float s;
        public int seg;
        public float t;
    }

    public void Play(Action onDone)
    {
        if (playing)
            return;

        if (introCamera == null || mainCamera == null || cinemachineBrain == null || vcamStart == null || gameplayCameraStart == null || waypointsParent == null)
        {
            onDone?.Invoke();
            return;
        }

        if (waypointsParent.childCount < 1)
        {
            StartCoroutine(EndFlyoverRoutine(onDone));
            return;
        }

        playing = true;

        if (gameplayCameraFollow != null)
            gameplayCameraFollow.enabled = false;

        cinemachineBrain.enabled = false;
        if (mainCamera != null) mainCamera.enabled = false;
        introCamera.enabled = true;

        StartCoroutine(Fly(onDone));
    }

    private IEnumerator Fly(Action onDone)
    {
        List<Waypoint> points = BuildPointList();
        int count = points.Count;
        int segCount = count - 1;

        if (segCount <= 0)
        {
            introCamera.transform.SetPositionAndRotation(points[0].pos, points[0].rot);
            yield return EndFlyoverRoutine(onDone);
            yield break;
        }

        if (secondsPerSegment <= 0f)
        {
            introCamera.transform.SetPositionAndRotation(points[count - 1].pos, points[count - 1].rot);
            yield return EndFlyoverRoutine(onDone);
            yield break;
        }

        List<Sample> table = new List<Sample>(segCount * Mathf.Max(2, samplesPerSegment) + 1);
        float totalLength = BuildArcLengthTable(points, segCount, table);

        float totalDuration = secondsPerSegment * segCount;
        float speed = totalLength / totalDuration;

        float traveled = 0f;

        EvaluateAtDistance(points, segCount, table, 0f, out Vector3 startPos, out Quaternion startRot);
        introCamera.transform.SetPositionAndRotation(startPos, startRot);

        while (traveled < totalLength)
        {
            traveled += speed * Time.deltaTime;
            float s = Mathf.Min(traveled, totalLength);

            EvaluateAtDistance(points, segCount, table, s, out Vector3 pos, out Quaternion rot);
            introCamera.transform.SetPositionAndRotation(pos, rot);

            yield return null;
        }

        introCamera.transform.SetPositionAndRotation(points[count - 1].pos, points[count - 1].rot);

        yield return EndFlyoverRoutine(onDone);
    }

    private List<Waypoint> BuildPointList()
    {
        int childCount = waypointsParent.childCount;
        List<Waypoint> points = new List<Waypoint>(childCount + 1);

        for (int i = 0; i < childCount; i++)
        {
            Transform t = waypointsParent.GetChild(i);
            points.Add(new Waypoint { pos = t.position, rot = t.rotation });
        }

        ComputeGameplayPose(out Vector3 endPos, out Quaternion endRot);
        points.Add(new Waypoint { pos = endPos, rot = endRot });

        return points;
    }

    private float BuildArcLengthTable(List<Waypoint> points, int segCount, List<Sample> table)
    {
        table.Clear();

        float total = 0f;
        table.Add(new Sample { s = 0f, seg = 0, t = 0f });

        Vector3 prev = EvalPosition(points, segCount, 0, 0f);

        int sps = Mathf.Max(2, samplesPerSegment);

        for (int seg = 0; seg < segCount; seg++)
        {
            for (int i = 1; i <= sps; i++)
            {
                float t = i / (float)sps;
                Vector3 p = EvalPosition(points, segCount, seg, t);
                total += Vector3.Distance(prev, p);
                prev = p;

                table.Add(new Sample { s = total, seg = seg, t = t });
            }
        }

        return total;
    }

    private void EvaluateAtDistance(List<Waypoint> points, int segCount, List<Sample> table, float s, out Vector3 pos, out Quaternion rot)
    {
        if (s <= 0f)
        {
            pos = EvalPosition(points, segCount, 0, 0f);
            rot = EvalRotation(points, segCount, 0, 0f);
            return;
        }

        int last = table.Count - 1;
        if (s >= table[last].s)
        {
            int seg = segCount - 1;
            pos = EvalPosition(points, segCount, seg, 1f);
            rot = EvalRotation(points, segCount, seg, 1f);
            return;
        }

        int lo = 0;
        int hi = last;

        while (lo < hi)
        {
            int mid = (lo + hi) >> 1;
            if (table[mid].s < s) lo = mid + 1;
            else hi = mid;
        }

        int idx = Mathf.Clamp(lo, 1, last);
        Sample b = table[idx];
        Sample a = table[idx - 1];

        float span = b.s - a.s;
        float u = span > 0f ? (s - a.s) / span : 0f;

        float t = Mathf.Lerp(a.t, b.t, u);
        int segIndex = a.seg;

        if (b.seg != a.seg)
        {
            segIndex = b.seg;
            t = Mathf.Lerp(0f, b.t, u);
        }

        pos = EvalPosition(points, segCount, segIndex, t);
        rot = EvalRotation(points, segCount, segIndex, t);
    }

    private Vector3 EvalPosition(List<Waypoint> points, int segCount, int seg, float t)
    {
        int count = points.Count;

        Waypoint p0 = points[Mathf.Clamp(seg - 1, 0, count - 1)];
        Waypoint p1 = points[Mathf.Clamp(seg, 0, count - 1)];
        Waypoint p2 = points[Mathf.Clamp(seg + 1, 0, count - 1)];
        Waypoint p3 = points[Mathf.Clamp(seg + 2, 0, count - 1)];

        return CatmullRom(p0.pos, p1.pos, p2.pos, p3.pos, t);
    }

    private Quaternion EvalRotation(List<Waypoint> points, int segCount, int seg, float t)
    {
        int count = points.Count;

        Vector3 pos = EvalPosition(points, segCount, seg, t);

        float dt = 0.0025f;
        float t2 = Mathf.Min(1f, t + dt);
        Vector3 pos2 = EvalPosition(points, segCount, seg, t2);

        Vector3 forward = (pos2 - pos);
        if (forward.sqrMagnitude < 1e-8f)
            forward = Vector3.forward;
        else
            forward.Normalize();

        Vector3 up = fallbackUp;

        if (useWaypointUp)
        {
            Waypoint a = points[Mathf.Clamp(seg, 0, count - 1)];
            Waypoint b = points[Mathf.Clamp(seg + 1, 0, count - 1)];
            Quaternion q = Quaternion.Slerp(a.rot, b.rot, t);
            up = q * Vector3.up;

            if (up.sqrMagnitude < 1e-8f) up = fallbackUp;
            else up.Normalize();
        }

        return Quaternion.LookRotation(forward, up);
    }

    private static Vector3 CatmullRom(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        float t2 = t * t;
        float t3 = t2 * t;

        return 0.5f * (
            (2f * p1) +
            (-p0 + p2) * t +
            (2f * p0 - 5f * p1 + 4f * p2 - p3) * t2 +
            (-p0 + 3f * p1 - 3f * p2 + p3) * t3
        );
    }

    private void ComputeGameplayPose(out Vector3 pos, out Quaternion rot)
    {
        pos = gameplayCameraStart.position;
        rot = gameplayCameraStart.rotation;

        if (gameplayCameraFollow == null || gameplayCameraFollow.player == null)
            return;

        Transform p = gameplayCameraFollow.player.transform;

        float desiredYAngle = p.eulerAngles.y;
        Quaternion yRot = Quaternion.Euler(0f, desiredYAngle, 0f);

        pos = p.position + yRot * gameplayCameraFollow.offset;

        Vector3 toPlayer = (p.position - pos);
        if (toPlayer.sqrMagnitude < 1e-8f)
        {
            rot = yRot;
            return;
        }

        rot = Quaternion.LookRotation(toPlayer.normalized, fallbackUp);
    }

    private IEnumerator EndFlyoverRoutine(Action onDone)
    {
        ComputeGameplayPose(out Vector3 endPos, out Quaternion endRot);

        if (introCamera != null)
            introCamera.transform.SetPositionAndRotation(endPos, endRot);

        if (introCamera != null)
            introCamera.enabled = false;

        if (vcamStart != null)
            vcamStart.transform.SetPositionAndRotation(endPos, endRot);

        if (mainCamera != null)
            mainCamera.transform.SetPositionAndRotation(endPos, endRot);

        if (mainCamera != null)
            mainCamera.enabled = true;

        if (cinemachineBrain != null)
            cinemachineBrain.enabled = false;

        yield return null;

        if (cinemachineBrain != null)
            cinemachineBrain.enabled = true;

        if (gameplayCameraFollow != null)
            gameplayCameraFollow.enabled = true;

        playing = false;
        onDone?.Invoke();
    }
}