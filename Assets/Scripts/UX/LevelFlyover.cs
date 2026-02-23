using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class LevelFlyover : MonoBehaviour
{
    [Header("Assign These")]
    [SerializeField] private CinemachineVirtualCamera vcamFlyover;
    [SerializeField] private CinemachineVirtualCamera vcamMain;
    [SerializeField] private Transform waypointsParent;

    [Header("Optional: last waypoint anchor (STATIC transform, not a live vcam)")]
    [SerializeField] private Transform gameplayCameraStart;

    [Header("Priorities")]
    [SerializeField] private int flyoverPriority = 20;
    [SerializeField] private int mainPriority = 10;

    [Header("Tuning")]
    [SerializeField] private float secondsPerSegment = 1.5f;

    [Header("Spline Sampling")]
    [SerializeField] private int samplesPerSegment = 40;

    [Header("End Rotation Stabilizer")]
    [Range(0f, 0.5f)]
    [SerializeField] private float endBlendPercent = 0.12f;

    [Header("Orientation")]
    [SerializeField] private bool useWaypointUp = true;
    [SerializeField] private Vector3 fallbackUp = Vector3.up;

    private bool playing;

    private int originalFlyoverPriority;
    private int originalMainPriority;

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

        if (vcamFlyover == null || vcamMain == null || waypointsParent == null)
        {
            onDone?.Invoke();
            return;
        }

        if (fallbackUp == Vector3.zero)
            fallbackUp = Vector3.up;

        if (waypointsParent.childCount < 1)
        {
            onDone?.Invoke();
            return;
        }

        playing = true;

        originalFlyoverPriority = vcamFlyover.Priority;
        originalMainPriority = vcamMain.Priority;

        vcamFlyover.gameObject.SetActive(true);

        vcamFlyover.Priority = flyoverPriority;
        vcamMain.Priority = mainPriority;

        StartCoroutine(Fly(onDone));
    }

    private IEnumerator Fly(Action onDone)
    {
        List<Waypoint> points = BuildPointList();
        int count = points.Count;
        int segCount = count - 1;

        if (segCount <= 0)
        {
            vcamFlyover.transform.SetPositionAndRotation(points[0].pos, points[0].rot);
            yield return EndFlyoverRoutine(onDone);
            yield break;
        }

        if (secondsPerSegment <= 0f)
        {
            vcamFlyover.transform.SetPositionAndRotation(points[count - 1].pos, points[count - 1].rot);
            yield return EndFlyoverRoutine(onDone);
            yield break;
        }

        int sps = Mathf.Max(2, samplesPerSegment);

        List<Sample> table = new List<Sample>(segCount * sps + 1);
        float totalLength = BuildArcLengthTable(points, segCount, table, sps);

        float totalDuration = secondsPerSegment * segCount;
        float speed = totalDuration > 0f ? totalLength / totalDuration : 0f;

        float traveled = 0f;

        Waypoint lastWp = points[count - 1];

        EvaluateAtDistance(points, segCount, table, 0f, out Vector3 startPos, out Quaternion startRot);
        vcamFlyover.transform.SetPositionAndRotation(startPos, startRot);

        float endBlendStart = Mathf.Max(0f, totalLength * (1f - Mathf.Clamp01(endBlendPercent)));

        while (traveled < totalLength)
        {
            traveled += speed * Time.deltaTime;
            float s = Mathf.Min(traveled, totalLength);

            EvaluateAtDistance(points, segCount, table, s, out Vector3 pos, out Quaternion rot);

            if (s >= endBlendStart && totalLength > 0f)
            {
                float u = Mathf.InverseLerp(endBlendStart, totalLength, s);
                rot = Quaternion.Slerp(rot, lastWp.rot, u);
            }

            vcamFlyover.transform.SetPositionAndRotation(pos, rot);

            yield return null;
        }

        vcamFlyover.transform.SetPositionAndRotation(lastWp.pos, lastWp.rot);

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

        if (gameplayCameraStart != null)
            points.Add(new Waypoint { pos = gameplayCameraStart.position, rot = gameplayCameraStart.rotation });
        else
            points.Add(new Waypoint { pos = points[points.Count - 1].pos, rot = points[points.Count - 1].rot });

        return points;
    }

    private float BuildArcLengthTable(List<Waypoint> points, int segCount, List<Sample> table, int sps)
    {
        table.Clear();

        float total = 0f;
        table.Add(new Sample { s = 0f, seg = 0, t = 0f });

        Vector3 prev = EvalPosition(points, 0, 0f);

        for (int seg = 0; seg < segCount; seg++)
        {
            for (int i = 1; i <= sps; i++)
            {
                float t = i / (float)sps;
                Vector3 p = EvalPosition(points, seg, t);
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
            pos = EvalPosition(points, 0, 0f);
            rot = EvalRotation(points, 0, 0f);
            return;
        }

        int last = table.Count - 1;
        if (s >= table[last].s)
        {
            int seg = segCount - 1;
            pos = EvalPosition(points, seg, 1f);
            rot = EvalRotation(points, seg, 1f);
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

        int segIndex = a.seg;
        float t = Mathf.Lerp(a.t, b.t, u);

        if (b.seg != a.seg)
        {
            segIndex = b.seg;
            t = Mathf.Lerp(0f, b.t, u);
        }

        pos = EvalPosition(points, segIndex, t);
        rot = EvalRotation(points, segIndex, t);
    }

    private Vector3 EvalPosition(List<Waypoint> points, int seg, float t)
    {
        int count = points.Count;

        Waypoint p0 = points[Mathf.Clamp(seg - 1, 0, count - 1)];
        Waypoint p1 = points[Mathf.Clamp(seg, 0, count - 1)];
        Waypoint p2 = points[Mathf.Clamp(seg + 1, 0, count - 1)];
        Waypoint p3 = points[Mathf.Clamp(seg + 2, 0, count - 1)];

        return CatmullRom(p0.pos, p1.pos, p2.pos, p3.pos, t);
    }

    private Quaternion EvalRotation(List<Waypoint> points, int seg, float t)
    {
        int count = points.Count;

        Waypoint p0 = points[Mathf.Clamp(seg - 1, 0, count - 1)];
        Waypoint p1 = points[Mathf.Clamp(seg, 0, count - 1)];
        Waypoint p2 = points[Mathf.Clamp(seg + 1, 0, count - 1)];
        Waypoint p3 = points[Mathf.Clamp(seg + 2, 0, count - 1)];

        Vector3 forward = CatmullRomTangent(p0.pos, p1.pos, p2.pos, p3.pos, t);
        if (forward.sqrMagnitude < 1e-8f) forward = Vector3.forward;
        else forward.Normalize();

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

    private static Vector3 CatmullRomTangent(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        float t2 = t * t;

        return 0.5f * (
            (-p0 + p2) +
            2f * (2f * p0 - 5f * p1 + 4f * p2 - p3) * t +
            3f * (-p0 + 3f * p1 - 3f * p2 + p3) * t2
        );
    }

    private IEnumerator EndFlyoverRoutine(Action onDone)
    {
        vcamFlyover.Priority = mainPriority;
        vcamMain.Priority = flyoverPriority;

        yield return null;

        vcamFlyover.Priority = originalFlyoverPriority;
        vcamMain.Priority = originalMainPriority;

        vcamFlyover.gameObject.SetActive(false);

        playing = false;
        onDone?.Invoke();
    }
}