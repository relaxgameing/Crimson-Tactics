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

        // making sure the object is in unit scale
        meshRenderer.transform.localScale.Set(1, 1, 1);

        Bounds bounds = meshRenderer.bounds;

        float maxDim = Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z);
        float scale = 1.0f / maxDim;
        go.transform.localScale = new Vector3(scale, scale, scale);

        Transform anchor = meshRenderer.probeAnchor;
        anchor.position.Scale(Vector3.one * scale);

        meshRenderer.probeAnchor.SetPositionAndRotation(anchor.position, go.transform.rotation);
        EditorUtility.SetDirty(go);
    }
}
