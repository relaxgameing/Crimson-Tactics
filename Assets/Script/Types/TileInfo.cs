using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

[Serializable]
public class TileInfo {
    public TileController Tile { get; private set; }

    public List<GameObject> Obstacles { get; private set; }

    public bool isOccupied => Obstacles != null && Obstacles.Count > 0 && Obstacles.Any(val =>
        val.activeInHierarchy && val.CompareTag("Obstacle")) ;

    public Vector2Int cellNo => GridSystem.Instance.CellNumber(Tile.transform.position) ;

    public TileInfo(TileController tile) {
        this.Tile = tile;
        this.Obstacles = new List<GameObject>();
    }

    public TileInfo(TileController tile, List<GameObject> obstacles) {
        this.Tile = tile;
        this.Obstacles = obstacles;
        if (obstacles == null) {
            Obstacles = new();
        }
    }
}
