using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Quaternion = UnityEngine.Quaternion;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

public class GridSystem : MonoBehaviour {
    private static GridSystem _instance;

    [SerializeField] private int gridRows = 2;
    [SerializeField] private int gridCols = 2;
    [SerializeField] private int gridSize = 1;
    [SerializeField] private GameObject defaultTile;
    private bool _isEditing;

    private GameObject _selectedObstacle;
    // future optimization:
    // could have each tile ref of all the surrounding 8 tiles like a linklist
    // to avoid accessing this massive list;
    // or
    // add chunk based system
    [SerializeField] private List<GameObject> _tiles;

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

    #region Utils

    // returns grid cell number for a particular world position
    // Note: functino Ignores Height
    // could use scriptable objects for the gridSize handling
    public Vector2Int CellNumber(Vector3 worldPos) {
        return new Vector2Int(
            Mathf.FloorToInt(worldPos.x) / gridSize,
            Mathf.FloorToInt(worldPos.z) / gridSize);
    }


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
        return _tiles[(int)(cellNo.x * gridRows + cellNo.y)];
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
                GameObject spawnedTile = Instantiate(defaultTile, this.transform);
                spawnedTile.transform.SetPositionAndRotation(pos, Quaternion.identity);
                spawnedTile.name = $"cell_{i}_{j}";
                spawnedTile.tag = TagHandle.GetExistingTag("Tile").ToString();
                _tiles.Add(spawnedTile);
            }
        }
    }

    public void Clear() {
        foreach (GameObject tile in _tiles) {
#if UNITY_EDITOR
            DestroyImmediate(tile);
#else
                Destroy(tile);
#endif
        }
        _tiles.Clear();
    }

    #endregion

    #region PathFinding

    class Node {
        public Vector2Int pos;
        public int gCost; // cost for start -> cur
        public int hCost; // cost for cur -> end
        public Node cameFrom;

        public Node(Vector2Int pos, int gCost, int hCost, Node cameFrom) {
            this.pos = pos;
            this.gCost = gCost;
            this.hCost = hCost;
            this.cameFrom = cameFrom;
        }
    }

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



    // find the shortest path between cell coord A to B using A* Algo
    // using city block distance
    public List<Vector2> PathFromAToB(Vector2Int start, Vector2Int end) {
        if (!CellWithInGrid(start) || !CellWithInGrid(end)) {
            return null;
        }

        var nodeComparer = new NodeComparer();
        SortedSet<Node> notVisited = new SortedSet<Node>(nodeComparer);
        Dictionary<Vector2Int, int> visited = new Dictionary<Vector2Int, int>();

        notVisited.Add(new Node(start, 0, CityBlockDist(start, end), null));

        Node Destination = null;
        while (notVisited.Count > 0) {
            var cur = notVisited.Min;
            notVisited.Remove(cur);

            if (visited.TryGetValue(cur.pos, out int bestCost) && bestCost <= cur.gCost) {
                continue;
            }


            if (cur.pos == end) {
                Destination = cur;
                GameObject dest = GetTileFromCellNumber(cur.pos);
                if (dest.GetComponent<TileController>().IsOccupied)
                    Destination = cur.cameFrom;

                break;
            }

            if (!CellWithInGrid(cur.pos)) {
                continue;
            }


            // this part could be optimized for a chunk by having a set to store
            // all the cells which are occupied
            GameObject tile = GetTileFromCellNumber(cur.pos);
            if (cur.pos != start && tile.GetComponent<TileController>().IsOccupied) continue;

            visited[cur.pos] = cur.gCost;
            // add all 4 neighbours
            var nodePos = new Vector2Int(cur.pos.x, cur.pos.y);

            nodePos.x++; // x+1
            notVisited.Add(new Node(
                nodePos,
                cur.gCost + 1,
                CityBlockDist(nodePos, end),
                cur
            ));

            nodePos.x -= 2; // x-1
            notVisited.Add(new Node(
                nodePos,
                cur.gCost + 1,
                CityBlockDist(nodePos, end),
                cur
            ));
            nodePos.x++; // x

            nodePos.y++; // y + 1
            notVisited.Add(new Node(
                nodePos,
                cur.gCost + 1,
                CityBlockDist(nodePos, end),
                cur
            ));
            nodePos.y -= 2; // y -1
            notVisited.Add(new Node(
                nodePos,
                cur.gCost + 1,
                CityBlockDist(nodePos, end),
                cur
            ));

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
