using UnityEngine;
using UnityEngine.WSA;

public class PlayerController : MonoBehaviour , IInteractable {

    public bool Interact() {
        return false;
    }
    public bool InteractWith(IInteractable other) {
        if (other is TileController tile ) {
            Debug.Log("moving player to tile " + tile.CellNo);
           var path =  GridSystem.Instance.PathFromAToB(
                GridSystem.Instance.CellNumber(transform.position),
                GridSystem.Instance.CellNumber(tile.transform.position)
                );

                foreach (Vector2 tilePos in path) {
                    var cur = GridSystem.Instance.GetTileFromCellNumber(tilePos);
                    var controller = cur.GetComponent<TileController>();
                    controller.HighLightTile(true , Color.indianRed);
                }
        }
        return false;
    }
}
