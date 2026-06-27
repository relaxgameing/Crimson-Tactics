using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using Quaternion = UnityEngine.Quaternion;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

// Manages the whole grid system of the game and provides utility functions related to grid
public class GridSystem : MonoBehaviour {
    private static GridSystem _instance;

    [SerializeField] private int gridRows = 2;
    [SerializeField] private int gridCols = 2;
    [SerializeField] private int gridSize = 1;
    [SerializeField] private GameObject gridTilePrefab;
    [SerializeField] private GridData _gridData;

    private Dictionary<Vector2Int, TileInfo> _grid = new() ;

    public Vector2 GridDimension => new Vector2(gridRows, gridCols);

    public static GridSystem Instance {
        get {
            if (_instance is null) {
                // Try to find existing instance in scene
                _instance = FindAnyObjectByType<GridSystem>();

                // If still not found, create a new GameObject
                if (_instance is null) {
                    GameObject obj = new("GridSystem");
                    _instance = obj.AddComponent<GridSystem>();
                }
            }

            return _instance;
        }
    }

    private void Start() {
        // we need to sync the grid when the game starts because the _grid is not
        // passed from the editor to play mode as Dictionary is not serializable by unity
        _grid.Clear();
        SyncGrid();

        Debug.Log(_grid.Count);
    }

    private void OnValidate() {
        // this ensures that we dont do any kind of editor stuff when playing
        if (Application.isPlaying) {
            return;
        }
        if (_grid == null) {
            _grid = new();
        }

        if (_gridData == null) {
            Debug.LogError("_grid system not attached to scriptable Object");
        }

        if (gridTilePrefab == null) {
            Debug.LogError("_grid Tile prefab not set");
        }
        else {
            if (gridTilePrefab.GetComponent<TileController>().IsUnityNull()) {
                Debug.LogError("provided prefab doesnt have TileController");
            }
        }

    }

    // remove tiles which are not inside the current _grid and store the current
    // state of the grid including the modification made by the editor
    public void SyncGrid() {
        // removing tile that are out of bound now
        List<Vector2Int> toRemove = new List<Vector2Int>();
        foreach (var tile in _grid) {
            if (!CellWithInGrid(tile.Key)) {
                ResetTile(tile.Key);
                toRemove.Add(tile.Key);
            }
        }

        foreach (Vector2Int cellNo in toRemove) {
            _grid.Remove(cellNo);
        }

        // adding new tiles to our grid
        int newTilesAdded = 0 , newObstaclesAdded = 0;
        for (int i = 0; i < transform.childCount; i++) {
            GameObject child = transform.GetChild(i).gameObject;
            TileController tile = child.GetComponent<TileController>();
            if (tile == null) {
                continue;
            }

            List<GameObject> obstacle = new();
            for (int j = 0; j < tile.transform.childCount; j++) {
                GameObject tileChild = tile.transform.GetChild(j).gameObject;
                if (tileChild.CompareTag("Obstacle")) {
                    obstacle.Add(tileChild);
                    newObstaclesAdded++;
                }
            }
            _grid[tile.CellNo] = new TileInfo(tile, obstacle);
            newTilesAdded++;
        }

        Debug.Log($"Validated Grid: current grid count:{_grid.Count} |" +
                  $" tiles added {newTilesAdded} |" +
                  $" obstacles added {newObstaclesAdded} |" +
                  $"removed " + $":{toRemove.Count} |");
    }




    #region Utils

    // returns _grid cell number for a particular world position
    // Note: functino Ignores Height
    // could use scriptable objects for the gridSize handling
    public Vector2Int CellNumber(Vector3 worldPos) {
        return new Vector2Int(
            Mathf.FloorToInt(worldPos.x) / gridSize,
            Mathf.FloorToInt(worldPos.z) / gridSize);
    }


    // checks if the cell no is valid for current _grid size
    public bool CellWithInGrid(Vector2 cellNo) {
        if (cellNo.y < 0 || cellNo.y >= gridCols) {
            return false;
        }

        if (cellNo.x < 0 || cellNo.x >= gridRows) {
            return false;
        }

        return true;
    }


    // returns tile gameobject from a given cell number
    public TileInfo GetTileInfoOfCellNumber(Vector2 cellNo) {
        _grid.TryGetValue(TypeConvertor.Vector2ToVector2Int(cellNo), out TileInfo info);
        if (info != null) {
            return info;
        }

        return null;
    }

    public bool IsCellOccupied(Vector2Int cell) {
        var info = _grid[cell];
        // if cell is null means that it is not valid so it would not allow anything to be placed
        if (info == null) {
            return true;
        }

        return info.isOccupied;
    }

    public void ResetTile(Vector2Int cellNo) {
        _grid.TryGetValue(cellNo, out TileInfo info);
        if (info == null) {
            return;
        }
#if UNITY_EDITOR
        if (info.Tile) {
            DestroyImmediate(info.Tile.gameObject);
        }

        foreach (GameObject go in info.Obstacles) {
            DestroyImmediate(go);
        }
#else
        Destroy(info.tile);
        foreach (GameObject go in info.obstacles) {
            Destroy(go);
        }
#endif
    }

    #endregion


    #region GridGeneration

    private TileController InstantiateTile(GameObject tilePrefab , Transform parent , Vector3 pos) {
        TileController spawnedTile = PrefabUtility.InstantiatePrefab(tilePrefab, parent)
            .GetComponent<TileController>();
        spawnedTile.transform.SetPositionAndRotation(pos + tilePrefab.transform.position, Quaternion
            .identity);
        spawnedTile.name = $"cell_{(int)pos.x}_{(int)pos.z}";
        spawnedTile.tag = TagHandle.GetExistingTag("Tile").ToString();

        return spawnedTile;
    }

    private List<GameObject> InstantiateObstacles(List<GameObject> obstaclesPrefab, Transform
            parent ) {
        List<GameObject> obstacles = new(obstaclesPrefab.Count);
        foreach (GameObject ob in obstaclesPrefab) {
            var obstacle = (GameObject)PrefabUtility.InstantiatePrefab(ob, parent);
            obstacles.Add(obstacle);
        }

        return obstacles;
    }

    public void Generate() {
        Clear();

        Debug.Log("Grid creation");
        Vector3 pos = Vector3.zero;
        pos.y = -1;
        for (int i = 0; i < gridRows; i++) {
            pos.x = i * gridSize;
            for (int j = 0; j < gridCols; j++) {
                pos.z = j * gridSize;
                var spawnedTile = InstantiateTile(gridTilePrefab , transform , pos);
                _grid.Add(new((int)pos.x, (int)pos.z), new TileInfo(spawnedTile));
            }
        }
    }


    // Editor only
    public void Clear() {
        Debug.Log($"Clearing Grid: count {_grid.Count}");

        foreach (var tileInfo in _grid) {
            ResetTile(tileInfo.Key);
        }

        for (int i = 0 ; i < transform.childCount ; i++) {
            GameObject child = transform.GetChild(i).gameObject;
#if UNITY_EDITOR
            DestroyImmediate(child);
#else
            Destroy(child);
#endif
        }
        _grid.Clear();
    }

    public void LoadGridFromScriptableObject() {
        if (_gridData == null) {
            Debug.LogWarning("grid Data scriptable object is not set");
            return;
        }

        // clear current grid and load the new one
        Clear();

        // getting the saved data
        var gridData = _gridData.GetGridData();
        Debug.Log("Loaded Grid successfully");

        // replacing the saved data with new instance of the data
        foreach (var data in gridData) {
            var pos =   new Vector3(
                data.position.x * gridSize ,
                0,
                data.position.y * gridSize);

            var tile = InstantiateTile(data.tilePrefab , transform, pos);
            List<GameObject> obstacles = InstantiateObstacles(data.obstaclePrefabs,
                tile.transform);

            _grid[data.position] = new TileInfo(tile, obstacles);
        }
    }

    public void SaveGridToScriptableObject() {
        if (_gridData == null) {
            Debug.LogWarning("grid Data scriptable object is not set");
            return;
        }

        if (_grid == null) {
            Debug.LogWarning("Can not save null grid to asset");
            return;
        }

        SyncGrid();
        if (_gridData.SaveGrid(_grid)) {
            Debug.Log("Save Grid successfully");
        }
    }

    #endregion

    #region GridEditorTool

    public void UpdateGridChanges( Vector2Int cell , Stack<ObstaclesChange> changes) {
        if (Application.isPlaying) {
            Debug.LogWarning("UpdateGridChanges should'nt be call in playmode");
            return;
        }

        if (!_grid.TryGetValue(cell, out TileInfo info)) {
            return;
        }

        // we will go throught the sequence of change and get the last value persistant at the end
        TileInfo tempInfo = new(info.Tile);
        foreach (ObstaclesChange change in changes) {
            switch (change.op) {
                case GridOperation.Adding:
                    tempInfo.Obstacles.Add(change.instance);
                    break;
                case GridOperation.Removing:
                    tempInfo.Obstacles.Remove(change.instance);
#if UNITY_EDITOR
                    DestroyImmediate(change.instance);
#else
                    Destroy(change.instance);
#endif
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        // inserting new fresh data
        _grid[cell] = tempInfo;
    }


    #endregion
}
