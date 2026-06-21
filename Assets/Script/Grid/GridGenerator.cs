using System;
using UnityEngine;

public class GridGenerator : MonoBehaviour {
    [SerializeField] private int gridRows = 10 ;
    [SerializeField] private int gridCols = 10;
    [SerializeField] private int gridSize = 1;
    [SerializeField] private GameObject defaultTile;


    public void Generate() {
        Clear();

        Debug.Log("Grid creation");
        Vector3 pos = Vector3.zero;
        pos.y = -1;
        for (int i = 0; i < gridRows; i++) {
            pos.x = i * gridSize;
            for (int j = 0; j < gridCols; j++) {
                pos.z = j * gridSize;
                var spawnedTile = Instantiate(defaultTile , transform);
                spawnedTile.transform.SetPositionAndRotation(pos , Quaternion.identity);
                spawnedTile.name = $"cell_{i}_{j}";
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
}
