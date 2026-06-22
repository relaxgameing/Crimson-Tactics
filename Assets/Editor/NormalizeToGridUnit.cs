using UnityEditor;
using UnityEngine;

public class NormalizeToGridUnit : EditorWindow {
    [MenuItem("Assets/Normalize Prefabs to 1 Unit")]
    public static void NormalizePrefabs() {
        var targets = Selection.objects;
        foreach (Object obj in targets) {
            if (obj is GameObject go) {
                NormalizeToOneUnit(go);
            }
        }
    }

    public static void NormalizeToOneUnit(GameObject go) {
        MeshRenderer meshRenderer = go.GetComponent<MeshRenderer>();
        if (meshRenderer == null) return;

        Bounds bounds = meshRenderer.bounds;
        float maxDim = Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z);
        float scale = 1.0f / maxDim;

        go.transform.localScale = new Vector3(scale, scale, scale);
        EditorUtility.SetDirty(go);
    }
}
