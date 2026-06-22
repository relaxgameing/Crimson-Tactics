using UnityEngine;

public class PlayerController : MonoBehaviour , IInteractable {

    public bool Interact() {
        return false;
    }
    public bool InteractWith(IInteractable other) {
        if (other is TileController tile ) {
            Debug.Log("moving player to tile " + tile.CellNo);
        }
        return false;
    }
}
