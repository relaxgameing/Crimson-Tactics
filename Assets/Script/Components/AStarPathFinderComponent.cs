using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MoverComponent))]
public class AStarPathFinderComponent :  PathFinder {
    [SerializeField] private Vector2Int[] movementDirections = new[] {
        Vector2Int.up,
        Vector2Int.down,
        Vector2Int.left,
        Vector2Int.right,
    };


    int CityBlockDist(Vector2 a, Vector2 b) {
        return (int)Mathf.Abs(a.x - b.x) + (int)Mathf.Abs(a.y - b.y);
    }

    class Node {
        public readonly Vector2Int Pos; // cur position
        public readonly int GCost; // cost for start -> cur
        public readonly int HCost; // cost for cur -> end
        public readonly Node CameFrom; // from which tile we have reached the current one

        public Node(Vector2Int pos, int gCost, int hCost, Node cameFrom) {
            this.Pos = pos;
            this.GCost = gCost;
            this.HCost = hCost;
            this.CameFrom = cameFrom;
        }
    }

    // custom comparer for the hashset to work with our algo
    class NodeComparer : IComparer<Node> {
        public int Compare(Node x, Node y) {
            if (x == null) return 1;
            if (y == null) return -1;

            int xCost = x.GCost + x.HCost;
            int yCost = y.GCost + y.HCost;

            if (xCost == yCost) {
                if (x.HCost < y.HCost) {
                    return -1;
                }

                if (x.HCost == y.HCost) {
                    if (x.GCost < y.GCost) {
                        return -1;
                    }

                    // this whole 'if' is handling the case where both node has same
                    // h and g cost. making us rely on cell coordinate as cell coordinate
                    // will always be different for different cell
                    if (x.GCost == y.GCost) {
                        if (x.Pos.x.CompareTo(y.Pos.x) != 0) {
                            return x.Pos.x.CompareTo(y.Pos.x);
                        }

                        return x.Pos.y.CompareTo(y.Pos.y);
                    }
                }

                return 1;
            }

            return xCost < yCost ? -1 : 1;
        }
    }

    public override List<Vector2> PathFromAToB(Vector2Int start, Vector2Int end) {
        return PathFromAToB(start, end, 0, true);
    }

    public override List<Vector2> PathFromAToB(Vector2Int start, Vector2Int end, int radius,
        bool strictRadius) {
        if (!GridSystem.Instance.CellWithInGrid(start) ||
            !GridSystem.Instance.CellWithInGrid(end) ||
            start == end) {
            return null;
        }

        TileInfo dest = GridSystem.Instance.GetTileInfoOfCellNumber(end);
        // these are the guarantee cases where skipping is good
        if (dest == null || (dest.isOccupied && radius == 0))
            return null;

        SortedSet<Node> notVisited = new SortedSet<Node>(new NodeComparer());
        Dictionary<Vector2Int, int> visited = new Dictionary<Vector2Int, int>();

        notVisited.Add(new Node(start, 0, CityBlockDist(start, end), null));

        Node destination = null;
        while (notVisited.Count > 0) {
            var cur = notVisited.Min;
            notVisited.Remove(cur);

            // checking if the current node is viable
            if (!GridSystem.Instance.CellWithInGrid(cur.Pos)) {
                continue;
            }

            // this part could be optimized for a chunk by having a set to store
            // all the cells which are occupied
            TileInfo tileInfo = GridSystem.Instance.GetTileInfoOfCellNumber(cur.Pos);
            if (cur.Pos != start && tileInfo.isOccupied)
                continue;


            if (visited.TryGetValue(cur.Pos, out int bestCost) && bestCost <= cur.GCost) {
                continue;
            }

            int dist = CityBlockDist(cur.Pos, end);
            if (dist == radius || (!strictRadius && dist < radius)) {
                if (destination == null || destination.GCost > cur.GCost) {
                    destination = cur;
                }

                continue;
            }


            visited[cur.Pos] = cur.GCost;
            // add all 4 neighbours
            foreach (Vector2Int dir in movementDirections) {
                if (!GridSystem.Instance.CellWithInGrid(cur.Pos + dir)) {
                    continue;
                }

                notVisited.Add(new Node(
                    cur.Pos + dir,
                    cur.GCost + 1,
                    CityBlockDist(cur.Pos + dir, end),
                    cur
                ));
            }

        }

        if (destination == null) {
            return null;
        }

        List<Vector2> path = new List<Vector2>();
        Node curNode = destination;
        while (curNode.CameFrom != null) {
            path.Add(curNode.Pos);
            curNode = curNode.CameFrom;
        }

        path.Add(curNode.Pos);
        path.Reverse();
        return path;
    }
}
