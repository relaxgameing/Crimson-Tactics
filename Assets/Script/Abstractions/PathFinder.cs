using System.Collections.Generic;
using UnityEngine;

public abstract class PathFinder : MonoBehaviour{

    [SerializeField] private Vector2 movementDirection;

    // find the shortest path between cell coord A to B
    public abstract List<Vector2> PathFromAToB(Vector2Int start, Vector2Int end);

    // find the shortest path between cell coord A to B
    // @param radius means how far away tile from destination is allowed or considered a valid
    // solution
    // @param strictRadius means that the destination can't be closer than radius
    public abstract List<Vector2> PathFromAToB(Vector2Int start, Vector2Int end, int radius, bool
        strictRadius);
}
