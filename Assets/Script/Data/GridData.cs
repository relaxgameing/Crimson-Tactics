using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[Serializable]
public class GridCell
{
    public Vector2Int position;
    public GameObject tilePrefab;
    public List<GameObject> obstaclePrefabs;

    public GridCell(Vector2Int position, GameObject tile , List<GameObject> obstacles)
    {
        this.position = position;
        this.tilePrefab = tile;
        this.obstaclePrefabs = obstacles;
    }
}

[CreateAssetMenu(fileName = "GridData", menuName = "Scriptable Object/Grid Data")]
public class GridData : ScriptableObject
{

    [SerializeField]
    private List<GridCell> cells = new();

    public IReadOnlyList<GridCell> Cells => cells;

    public bool SaveGrid(Dictionary<Vector2Int, TileInfo> grid) {

        if (grid == null)
            return false;

        List<GridCell> newData = new();

        foreach (var pair in grid) {
            var prefabTile = GetPrefab(pair.Value.Tile.gameObject);
            if (prefabTile == null) {
                Debug.LogError($"Tile at {pair.Key} is not a prefab instance. Cant Save");
                return false;
            }

            List<GameObject> prefabObstacles = new List<GameObject>();
            foreach (var obj in pair.Value.Obstacles) {
                var temp = GetPrefab(obj);
                if (temp == null) {
                    Debug.LogError($"Tile at {pair.Key} is not a prefab instance. Cant Save");
                    return false;
                }
                prefabObstacles.Add(temp);
            }
            newData.Add(new(
                pair.Key ,
                prefabTile,
                prefabObstacles));
        }

        cells = newData;

#if UNITY_EDITOR
        Debug.Log($"{cells.Count} cells saved to {name}");
        EditorUtility.SetDirty(this);
        AssetDatabase.SaveAssetIfDirty(this);
        AssetDatabase.Refresh();
#endif

        return true;
    }

    public IReadOnlyList<GridCell> GetGridData() {
        return cells;
    }

    public static GameObject GetPrefab(GameObject ob) {
        if (ob == null) {
            return null;
        }

        GameObject prefab = null;
#if UNITY_EDITOR
            prefab = PrefabUtility.GetCorrespondingObjectFromSource(ob);
#endif
        return prefab;
    }

    public void Clear()
    {
        cells?.Clear();

#if UNITY_EDITOR
        EditorUtility.SetDirty(this);
        AssetDatabase.SaveAssetIfDirty(this);
        AssetDatabase.Refresh();
#endif
    }
}
