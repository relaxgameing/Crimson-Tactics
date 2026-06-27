using UnityEngine;


public enum GridOperation {
    Adding,
    Removing,
}

public class ObstaclesChange {
    public GameObject prefab;
    public GameObject instance;
    public GridOperation op;

    public ObstaclesChange(GameObject prefab , GameObject instance , GridOperation op ) {
        this.instance = instance;
        this.prefab = prefab;
        this.op = op;
    }
}
