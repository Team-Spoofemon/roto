using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class WorldNote : MonoBehaviour
{
    [TextArea]
    public string noteText = "This is a note.";

    public Vector3 offset = Vector3.up * 2;

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        GUIStyle style = new GUIStyle();
        style.normal.textColor = Color.red;
        style.fontStyle = FontStyle.Bold;
        style.alignment = TextAnchor.UpperCenter;

        Handles.Label(transform.position + offset, noteText, style);
    }
#endif
}
