using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GridData))]
public class GridDataEditor : Editor
{
    private bool _showCells = true;

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        SerializedProperty cells =
            serializedObject.FindProperty("cells");

        EditorGUILayout.Space();

        EditorGUILayout.LabelField(
            "Grid Data",
            EditorStyles.boldLabel);

        EditorGUILayout.LabelField(
            "Cell Count",
            cells?.arraySize.ToString() ?? "0");

        EditorGUILayout.Space();

        if (GUILayout.Button("Clear Grid"))
        {
            if (EditorUtility.DisplayDialog(
                    "Clear Grid",
                    "Remove all cells from this GridData asset?",
                    "Yes",
                    "Cancel"))
            {
                Undo.RecordObject(target, "Clear Grid");

                ((GridData)target).Clear();

                serializedObject.Update();
            }
        }

        EditorGUILayout.Space();

        _showCells = EditorGUILayout.Foldout(
            _showCells,
            "Cells",
            true);

        if (_showCells && cells != null)
        {
            EditorGUI.indentLevel++;

            for (int i = 0; i < cells.arraySize; i++)
            {
                SerializedProperty cell =
                    cells.GetArrayElementAtIndex(i);

                if (cell == null)
                    continue;

                SerializedProperty position =
                    cell.FindPropertyRelative("position");

                SerializedProperty tile =
                    cell.FindPropertyRelative("tilePrefab");

                SerializedProperty obstacles =
                    cell.FindPropertyRelative("obstaclePrefabs");

                EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                EditorGUILayout.LabelField(
                    $"Cell {i}",
                    EditorStyles.boldLabel);

                if (position != null)
                    EditorGUILayout.PropertyField(position);

                if (tile != null)
                    EditorGUILayout.PropertyField(tile, true);
                else
                    EditorGUILayout.HelpBox("Tile reference is null.", MessageType.Warning);

                if (obstacles != null)
                    EditorGUILayout.PropertyField(obstacles, true);
                else
                    EditorGUILayout.HelpBox("obstacle reference is null.", MessageType.Warning);

                EditorGUILayout.EndVertical();

                EditorGUILayout.Space(2);
            }

            EditorGUI.indentLevel--;
        }

        serializedObject.ApplyModifiedProperties();
    }
}
