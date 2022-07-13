using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(RopeRoot))]
public class RopeEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        GUILayout.Space(8f);

        if (GUILayout.Button("Create Joints"))
        {
            RopeRoot ropeRoot = (RopeRoot)target;
            ropeRoot.Generate();
        }

        GUILayout.Space(8f);

        if (GUILayout.Button("Clear"))
        {
            RopeRoot ropeRoot = (RopeRoot)target;
            ropeRoot.Clear();
        }
    }
}
