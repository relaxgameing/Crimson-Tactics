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

    private Vector2Int[] _movementDirections = new[] {
        Vector2Int.up,
        Vector2Int.down,
        Vector2Int.left,
        Vector2Int.right,
    };

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

    int CityBlockDist(Vector2 a, Vector2 b) {
        return (int)Mathf.Abs(a.x - b.x) + (int)Mathf.Abs(a.y - b.y);
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

    #region PathFinding

    class Node {
        public Vector2Int pos; // cur position
        public int gCost; // cost for start -> cur
        public int hCost; // cost for cur -> end
        public Node cameFrom; // from which tile we have reached the current one

        public Node(Vector2Int pos, int gCost, int hCost, Node cameFrom) {
            this.pos = pos;
            this.gCost = gCost;
            this.hCost = hCost;
            this.cameFrom = cameFrom;
        }
    }

    // custom comparer for the hashset to work with our algo
    class NodeComparer : IComparer<Node> {
        public int Compare(Node x, Node y) {
            if (x == null) return 1;
            if (y == null) return -1;

            int xCost = x.gCost + x.hCost;
            int yCost = y.gCost + y.hCost;

            if (xCost == yCost) {
                if (x.hCost < y.hCost) {
                    return -1;
                }

                if (x.hCost == y.hCost) {
                    if (x.gCost < y.gCost) {
                        return -1;
                    }

                    // this whole 'if' is handling the case where both node has same
                    // h and g cost. making us rely on cell coordinate as cell coordinate
                    // will always be different for different cell
                    if (x.gCost == y.gCost) {
                        if (x.pos.x.CompareTo(y.pos.x) != 0) {
                            return x.pos.x.CompareTo(y.pos.x);
                        }

                        return x.pos.y.CompareTo(y.pos.y);
                    }
                }

                return 1;
            }

            return xCost < yCost ? -1 : 1;
        }
    }


    public List<Vector2> PathFromAToB(Vector2Int start, Vector2Int end) {
        return PathFromAToB(start, end, 0, true);
    }

    // find the shortest path between cell coord A to B using A* Algo
    // using city block distance
    // @param radius means how far away tile from destination is allowed or considered a valid
    // solution
    // @param strictRadius means that the destination can't be closer than radius
    public List<Vector2> PathFromAToB(Vector2Int start, Vector2Int end, int radius, bool
        strictRadius) {
        if (!CellWithInGrid(start) || !CellWithInGrid(end) || start == end) {
            return null;
        }

        GameObject dest = GetTileFromCellNumber(end);
        // these are the guarantee cases where skipping is good
        if (!dest || (dest.GetComponent<TileController>().IsOccupied && radius == 0))
            return null;

        SortedSet<Node> notVisited = new SortedSet<Node>(new NodeComparer());
        Dictionary<Vector2Int, int> visited = new Dictionary<Vector2Int, int>();

        notVisited.Add(new Node(start, 0, CityBlockDist(start, end), null));

        Node Destination = null;
        while (notVisited.Count > 0) {
            var cur = notVisited.Min;
            notVisited.Remove(cur);

            // checking if the current node is viable
            if (!CellWithInGrid(cur.pos)) {
                continue;
            }

            // this part could be optimized for a chunk by having a set to store
            // all the cells which are occupied
            GameObject tile = GetTileFromCellNumber(cur.pos);
            if (cur.pos != start && tile.GetComponent<TileController>().IsOccupied)
                continue;


            if (visited.TryGetValue(cur.pos, out int bestCost) && bestCost <= cur.gCost) {
                continue;
            }

            int dist = CityBlockDist(cur.pos, end);
            if (dist == radius || (!strictRadius && dist < radius)) {
                if (Destination == null || Destination.gCost > cur.gCost) {
                    Destination = cur;
                }

                continue;
            }


            visited[cur.pos] = cur.gCost;
            // add all 4 neighbours
            foreach (Vector2Int dir in _movementDirections) {
                if (!CellWithInGrid(cur.pos + dir)) {
                    continue;
                }

                notVisited.Add(new Node(
                    cur.pos + dir,
                    cur.gCost + 1,
                    CityBlockDist(cur.pos + dir, end),
                    cur
                ));
            }

        }

        if (Destination == null) {
            return null;
        }

        List<Vector2> path = new List<Vector2>();
        Node curNode = Destination;
        while (curNode.cameFrom != null) {
            path.Add(curNode.pos);
            curNode = curNode.cameFrom;
        }

        path.Add(curNode.pos);
        path.Reverse();
        return path;
    }

    #endregion

}
