using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour, IInteractable {
    private MoverComponent _mover;

    private void Awake() {
        _mover = GetComponent<MoverComponent>();
    }

    public bool Interact() {
        return false;
    }

    public bool InteractWith(IInteractable other) {
        if (other is TileController tile) {
            // Debug.Log("moving player to tile " + tile.CellNo);
            var path = _mover.FindNewPath(
                GridSystem.Instance.CellNumber(transform.position),
                GridSystem.Instance.CellNumber(tile.transform.position),
                0 , true
            );

            if (path == null) {
                Debug.Log("Tile Unreachable");
                return false;
            }

            _mover.WhenPathChangeComplete(() => {
                Debug.Log("start moving player");
                _mover.StartMoving();
            });
            _mover.ChangePath(path);
        }

        GameModeController.Instance.AddObjectSimulating(gameObject);

        return true;
    }
}
