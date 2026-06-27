using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class TileInfo {
    public TileController Tile { get; private set; }

    public List<GameObject> Obstacles { get; private set; }

    public bool isOccupied => Obstacles != null && Obstacles.Count > 0;

    public Vector2Int cellNo => new(Mathf.FloorToInt(Tile.transform.position.x),
        Mathf.FloorToInt(Tile.transform.position.z));

    public TileInfo(TileController tile) {
        this.Tile = tile;
        this.Obstacles = new List<GameObject>();
    }

    public TileInfo(TileController tile, List<GameObject> obstacles) {
        this.Tile = tile;
        this.Obstacles = obstacles;
    }

    public void ClearInfo() {

    }
}
