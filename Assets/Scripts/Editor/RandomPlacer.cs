using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class RandomPlacer : EditorWindow
{
    [System.Serializable]
    private class PrefabEntry
    {
        public GameObject prefab;
        public float weight = 1f;
    }

    private PrefabEntry[] prefabs = new PrefabEntry[5]
    {
        new PrefabEntry(), new PrefabEntry(), new PrefabEntry(), new PrefabEntry(), new PrefabEntry()
    };

    private Vector2 areaSize = new Vector2(50, 50);
    private Vector3 center = Vector3.zero;
    private GameObject anchorObject;

    private float baseHeight = 0f;
    private int count = 50;
    private bool alignToGround = true;
    private bool randomizeRotation = true;
    private bool avoidColliders = true;
    private float startScale = 1f;
    private float endScale = 1f;

    [MenuItem("Window/Random Placer")]
    public static void ShowWindow() => GetWindow<RandomPlacer>("Random Placer");

    void OnGUI()
    {
        GUILayout.Space(5);
        GUILayout.Label("🎯 Random Object Placer", EditorStyles.boldLabel);

        DrawSection("Prefab Variations", DrawPrefabSection);
        DrawSection("Placement Settings", DrawPlacementSection);
        DrawSection("Scale Settings", DrawScaleSection);

        GUILayout.Space(10);
        if (GUILayout.Button("PLACE OBJECTS", GUILayout.Height(35)))
            PlaceObjects();
    }

    // ---------- UI Layout ----------
    void DrawSection(string title, System.Action content)
    {
        GUILayout.Space(4);
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        GUILayout.Label(title, EditorStyles.boldLabel);
        content.Invoke();
        EditorGUILayout.EndVertical();
    }

    void DrawPrefabSection()
    {
        for (int i = 0; i < prefabs.Length; i++)
        {
            EditorGUILayout.BeginHorizontal();
            prefabs[i].prefab = (GameObject)EditorGUILayout.ObjectField($"Prefab {i + 1}", prefabs[i].prefab, typeof(GameObject), false);
            prefabs[i].weight = EditorGUILayout.Slider(prefabs[i].weight, 0f, 10f, GUILayout.Width(150));
            EditorGUILayout.EndHorizontal();
        }
    }

    void DrawPlacementSection()
    {
        areaSize = EditorGUILayout.Vector2Field("Area (Width, Depth)", areaSize);
        center = EditorGUILayout.Vector3Field("Area Center", center);

        anchorObject = (GameObject)EditorGUILayout.ObjectField("Match From Object", anchorObject, typeof(GameObject), true);
        if (anchorObject != null && GUILayout.Button("Match Position & Area", GUILayout.Height(22)))
        {
            center = anchorObject.transform.position;
            Renderer r = anchorObject.GetComponent<Renderer>();
            if (r) areaSize = new Vector2(r.bounds.size.x, r.bounds.size.z);
            else Debug.LogWarning("No Renderer found — using existing area size.");
            SceneView.RepaintAll();
        }

        baseHeight = EditorGUILayout.FloatField("Base Height (Y Offset)", baseHeight);
        count = EditorGUILayout.IntField("Object Count", count);
        alignToGround = EditorGUILayout.Toggle("Align to Ground (Raycast)", alignToGround);
        randomizeRotation = EditorGUILayout.Toggle("Randomize Y Rotation", randomizeRotation);
        avoidColliders = EditorGUILayout.Toggle("Avoid Colliders When Placing", avoidColliders);
    }

    void DrawScaleSection()
    {
        startScale = EditorGUILayout.FloatField("Min Scale", startScale);
        endScale = EditorGUILayout.FloatField("Max Scale", endScale);
    }

    // ---------- Core Placement ----------
    void PlaceObjects()
    {
        // Validate prefabs
        List<GameObject> validPrefabs = new();
        List<float> validWeights = new();
        foreach (var p in prefabs)
        {
            if (p.prefab != null && p.weight > 0f)
            {
                validPrefabs.Add(p.prefab);
                validWeights.Add(p.weight);
            }
        }

        if (validPrefabs.Count == 0)
        {
            Debug.LogWarning("⚠️ No valid prefabs assigned!");
            return;
        }

        GameObject parent = new GameObject("RandomizedObjects");
        Undo.RegisterCreatedObjectUndo(parent, "Create Randomized Objects");

        int placed = 0, skipped = 0;

        for (int i = 0; i < count; i++)
        {
            GameObject prefab = PickWeighted(validPrefabs, validWeights);

            Vector3 pos = new Vector3(
                center.x + Random.Range(-areaSize.x / 2, areaSize.x / 2),
                center.y + 50f, // cast from above
                center.z + Random.Range(-areaSize.y / 2, areaSize.y / 2)
            );

            // Align to ground (raycast down)
            if (alignToGround)
            {
                if (Physics.Raycast(pos, Vector3.down, out RaycastHit hit, 1000f))
                    pos = hit.point + Vector3.up * baseHeight;
                else
                    pos.y = center.y + baseHeight;
            }
            else
            {
                pos.y = center.y + baseHeight;
            }

            // Skip if collider nearby
            if (avoidColliders && Physics.CheckSphere(pos, 0.5f))
            {
                skipped++;
                continue;
            }

            Quaternion rot = randomizeRotation
                ? Quaternion.Euler(0, Random.Range(0f, 360f), 0)
                : Quaternion.identity;

            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            Undo.RegisterCreatedObjectUndo(instance, "Place Object");

            instance.transform.SetPositionAndRotation(pos, rot);
            instance.transform.localScale = Vector3.one * Random.Range(startScale, endScale);
            instance.transform.SetParent(parent.transform, true);

            placed++;
        }

        Debug.Log($"✅ Placement complete: {placed} placed, {skipped} skipped (collisions).");
        Selection.activeGameObject = parent;
        SceneView.RepaintAll();
    }

    // ---------- Weighted Random ----------
    GameObject PickWeighted(List<GameObject> prefabs, List<float> weights)
    {
        float total = 0f;
        for (int i = 0; i < weights.Count; i++) total += weights[i];
        float r = Random.value * total, c = 0f;
        for (int i = 0; i < prefabs.Count; i++)
        {
            c += weights[i];
            if (r <= c) return prefabs[i];
        }
        return prefabs[^1];
    }

    // ---------- Scene Gizmo ----------
    void OnSceneGUI(SceneView sceneView)
    {
        Handles.color = new Color(0f, 1f, 0f, 0.7f);
        Handles.DrawWireCube(center, new Vector3(areaSize.x, 0.1f, areaSize.y));
    }

    void OnEnable() => SceneView.duringSceneGui += OnSceneGUI;
    void OnDisable() => SceneView.duringSceneGui -= OnSceneGUI;
}
