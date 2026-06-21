using System;
using UnityEngine;

public class GridGenerator : MonoBehaviour {
    [SerializeField] private int gridRows = 2 ;
    [SerializeField] private int gridCols = 2;
    [SerializeField] private int gridSize = 1;
    [SerializeField] private GameObject defaultTile;

    // returns grid cell number for a particular world position
    // Note: functino Ignores Height
    // could use scriptable objects for the gridSize handling
    public static Vector2 CellNumber(Vector3 worldPos , float gridSize = 1 ) {
        return new Vector2(
            Mathf.FloorToInt(worldPos.x) / gridSize,
            Mathf.FloorToInt(worldPos.z) / gridSize);
    }

    private void NormalizeTile() {
        var renderer = defaultTile.GetComponent<MeshRenderer>();
        float xScale = gridSize / renderer.bounds.size.x;
        float yScale = gridSize / renderer.bounds.size.y;

        defaultTile.transform.localScale.Set(xScale , yScale , xScale);
        defaultTile.tag = TagHandle.GetExistingTag("Tile").ToString();
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
                var spawnedTile = Instantiate(defaultTile , transform);
                spawnedTile.transform.SetPositionAndRotation(pos , Quaternion.identity);
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
}
