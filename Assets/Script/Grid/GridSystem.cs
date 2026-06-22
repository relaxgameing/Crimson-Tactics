using UnityEngine;

public class GridSystem : MonoBehaviour {
    [SerializeField] private int gridRows = 2;
    [SerializeField] private int gridCols = 2;
    [SerializeField] private int gridSize = 1;
    [SerializeField] private GameObject defaultTile;
    private bool _isEditing;

    private GameObject _selectedObstacle;


    public Vector2 GridDimension => new Vector2(gridRows, gridCols);

    // returns grid cell number for a particular world position
    // Note: functino Ignores Height
    // could use scriptable objects for the gridSize handling
    public static Vector2 CellNumber(Vector3 worldPos, float gridSize = 1) {
        return new Vector2(
            Mathf.FloorToInt(worldPos.x) / gridSize,
            Mathf.FloorToInt(worldPos.z) / gridSize);
    }

    #region GridEditor

    public void SetObstacle(GameObject obj) {
        _selectedObstacle = obj;
        Debug.Log("Selected obstacle " + obj.name);
    }

    public void ToggleEditingMode() {
        _isEditing = !_isEditing;
    }

    #endregion


    #region GridGeneration

    public void Generate() {
        Clear();

        Debug.Log("Grid creation");
        Vector3 pos = Vector3.zero;
        pos.y = -1;
        for (int i = 0; i < gridRows; i++) {
            pos.x = i * gridSize;
            for (int j = 0; j < gridCols; j++) {
                pos.z = j * gridSize;
                GameObject spawnedTile = Instantiate(defaultTile, this.transform);
                spawnedTile.transform.SetPositionAndRotation(pos, Quaternion.identity);
                spawnedTile.name = $"cell_{i}_{j}";
                spawnedTile.tag = TagHandle.GetExistingTag("Tile").ToString();
            }
        }
    }

    public void Clear() {
        var tiles = GameObject.FindGameObjectsWithTag("Tile");
        foreach (GameObject tile in tiles) {
            #if UNITY_EDITOR
                DestroyImmediate(tile);
            #else
                Destroy(tile);
            #endif
        }
    }

    #endregion

}
