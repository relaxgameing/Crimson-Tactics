using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour, IInteractable {
    public bool Interact() {
        return false;
    }

    public bool InteractWith(IInteractable other) {
        if (other is TileController tile) {
            // Debug.Log("moving player to tile " + tile.CellNo);
            var path = GridSystem.Instance.PathFromAToB(
                GridSystem.Instance.CellNumber(transform.position),
                GridSystem.Instance.CellNumber(tile.transform.position)
            );

            if (path == null) {
                Debug.Log("Tile Unreachable");
                return false;
            }

            var mover = GetComponent<MoverComponent>();
            mover.WhenPathChangeComplete(() => {
                Debug.Log("start moving player");
                mover.StartMoving();
            });
            mover.ChangePath(path);
        }

        GameModeController.Instance.AddObjectSimulating(gameObject);

        return true;
    }
}
