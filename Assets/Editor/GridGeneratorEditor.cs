using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GridGenerator))]
public class GridGeneratorEditor : UnityEditor.Editor{

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        GridGenerator grid = (GridGenerator)target;
        GUILayout.Space(10);
        if (GUILayout.Button("Generate Grid"))
        {
            grid.Generate();
            EditorUtility.SetDirty(grid.gameObject);
        }
        if (GUILayout.Button("Clear Grid"))
        {
            grid.Clear();
            EditorUtility.SetDirty(grid.gameObject);
        }
    }
}
