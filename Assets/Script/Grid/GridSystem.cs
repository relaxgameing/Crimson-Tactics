using System;
using System.Collections.Generic;
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

    [SerializeField] private List<GameObject> gridTiles;

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
                    DontDestroyOnLoad(obj);
                }
            }

            return _instance;
        }
    }


    private void OnValidate() {
        if (gridTiles.Count == gridCols * gridRows) {
            return;
        }

        GameObject.FindGameObjectsWithTag("Tile", gridTiles);

        gridTiles.Sort(((a, ba) => {
            var aCell = CellNumber(a.transform.position);
            var bCell = CellNumber(ba.transform.position);

            if (aCell.x < bCell.x) {
                return -1;
            }

            if (aCell.x == bCell.x) {
                return aCell.y - bCell.y;
            }

            return 1;
        }));
    }

    #region Utils

    // returns grid cell number for a particular world position
    // Note: functino Ignores Height
    // could use scriptable objects for the gridSize handling
    public Vector2Int CellNumber(Vector3 worldPos) {
        return new Vector2Int(
            Mathf.FloorToInt(worldPos.x) / gridSize,
            Mathf.FloorToInt(worldPos.z) / gridSize);
    }


    // checks if the cell no is valid for current grid size
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
    public GameObject GetTileFromCellNumber(Vector2 cellNo) {
        return gridTiles[(int)(cellNo.x * gridRows + cellNo.y)];
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
                GameObject spawnedTile = Instantiate(gridTilePrefab, this.transform);
                spawnedTile.transform.SetPositionAndRotation(pos, Quaternion.identity);
                spawnedTile.name = $"cell_{i}_{j}";
                spawnedTile.tag = TagHandle.GetExistingTag("Tile").ToString();
                gridTiles.Add(spawnedTile);
            }
        }
    }


    public void Clear() {
        foreach (GameObject tile in gridTiles) {
#if UNITY_EDITOR
            DestroyImmediate(tile);
#else
                Destroy(tile);
#endif
        }

        gridTiles.Clear();
    }

    #endregion
}
