using UnityEngine;
using UnityEditor;

public class ScaleAdjuster : EditorWindow
{
    GameObject parent;
    bool applyToChildren = true;
    bool uniformScale = true;
    bool randomizeYRotation = false; // ✅ new toggle
    Vector3 minScale = Vector3.one;
    Vector3 maxScale = Vector3.one;

    [MenuItem("Window/Scale Adjuster")]
    public static void ShowWindow()
    {
        GetWindow<ScaleAdjuster>("Scale Adjuster");
    }

    void OnGUI()
    {
        GUILayout.Label("Scale Adjuster Tool", EditorStyles.boldLabel);

        parent = (GameObject)EditorGUILayout.ObjectField("Target Object", parent, typeof(GameObject), true);
        applyToChildren = EditorGUILayout.Toggle("Affect All Children", applyToChildren);
        uniformScale = EditorGUILayout.Toggle("Uniform Scale", uniformScale);
        randomizeYRotation = EditorGUILayout.Toggle("Randomize Y Rotation", randomizeYRotation); // ✅ new UI line

        EditorGUILayout.Space(10);
        GUILayout.Label("Scale Range", EditorStyles.boldLabel);

        if (uniformScale)
        {
            float min = EditorGUILayout.FloatField("Min Scale", minScale.x);
            float max = EditorGUILayout.FloatField("Max Scale", maxScale.x);
            minScale = Vector3.one * min;
            maxScale = Vector3.one * max;
        }
        else
        {
            minScale = EditorGUILayout.Vector3Field("Min Scale", minScale);
            maxScale = EditorGUILayout.Vector3Field("Max Scale", maxScale);
        }

        EditorGUILayout.Space(10);
        if (GUILayout.Button("Randomize Scales", GUILayout.Height(30)))
            RandomizeScales();

        if (GUILayout.Button("Reset to 1,1,1", GUILayout.Height(20)))
            ResetScales();
    }

    void RandomizeScales()
    {
        if (parent == null)
        {
            Debug.LogWarning("No target object selected!");
            return;
        }

        Undo.RegisterFullObjectHierarchyUndo(parent, "Randomize Scales");

        Transform[] targets = applyToChildren
            ? parent.GetComponentsInChildren<Transform>()
            : new Transform[] { parent.transform };

        foreach (Transform t in targets)
        {
            if (t == parent.transform && applyToChildren)
                continue; // skip parent container if desired

            // ✅ randomize scale
            if (uniformScale)
            {
                float s = Random.Range(minScale.x, maxScale.x);
                t.localScale = Vector3.one * s;
            }
            else
            {
                t.localScale = new Vector3(
                    Random.Range(minScale.x, maxScale.x),
                    Random.Range(minScale.y, maxScale.y),
                    Random.Range(minScale.z, maxScale.z)
                );
            }

            // ✅ optional random Y rotation
            if (randomizeYRotation)
            {
                Vector3 euler = t.localEulerAngles;
                euler.y = Random.Range(0f, 360f);
                t.localEulerAngles = euler;
            }
        }

        Debug.Log("Randomized scale" + (randomizeYRotation ? " and rotation" : "") + " for selected objects.");
    }

    void ResetScales()
    {
        if (parent == null)
        {
            Debug.LogWarning("No target object selected!");
            return;
        }

        Undo.RegisterFullObjectHierarchyUndo(parent, "Reset Scales");

        Transform[] targets = applyToChildren
            ? parent.GetComponentsInChildren<Transform>()
            : new Transform[] { parent.transform };

        foreach (Transform t in targets)
            t.localScale = Vector3.one;

        Debug.Log("Reset scales to (1,1,1).");
    }
}
