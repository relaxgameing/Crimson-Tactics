using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour, IInteractable {
    private Vector3 _lookAt = Vector3.forward;
    private List<TileController> _pathToTake;
    private int _nextTileIdx;
    private int _prevTileIdx;

    [SerializeField] private float movementSpeed = 0.7f;
    [SerializeField] private float rotationSpeed = 0.2f;
    private float _elapsedTime = 0;

    private void Awake() {
        _pathToTake = new List<TileController>(10);
    }

    private void Update() {
        // we move if we have a path
        MovePlayer();
    }

    void MovePlayer() {
        if (_pathToTake.Count <= 0) return;

        _elapsedTime += Time.deltaTime;

        var nextTile = _pathToTake[_nextTileIdx];
        var prevTile = _pathToTake[_prevTileIdx];
        Vector3 movementDir = nextTile.transform.position - prevTile.transform.position;

        float newRotation = Mathf.LerpAngle(
            Vector3.SignedAngle(Vector3.forward, _lookAt, Vector3.up),
            Vector3.SignedAngle(Vector3.forward, movementDir, Vector3.up),
            _elapsedTime / rotationSpeed
        );

        Vector3 newPos = Vector3.Lerp(prevTile.transform.position, nextTile.transform.position,
            _elapsedTime /
            movementSpeed);
        // we are not changing the y
        newPos.y = transform.position.y;

        Debug.Log("updating transform");
        transform.SetPositionAndRotation(newPos, Quaternion.Euler(0, newRotation, 0));

        if (_elapsedTime >= movementSpeed) {
            Debug.Log("one tile movement completed");
            _lookAt = nextTile.transform.position - prevTile.transform.position;
            _prevTileIdx = _nextTileIdx;
            _nextTileIdx++;
            _elapsedTime = 0;
        }

        if (_nextTileIdx >= _pathToTake.Count) {
            Debug.Log("path travel complete");
            foreach (TileController tile in _pathToTake) {
                tile.HighLightTile(false, Color.white);
            }

            _pathToTake.Clear();
            _elapsedTime = 0;
        }
    }

    public bool Interact() {
        return false;
    }

    public bool InteractWith(IInteractable other) {
        if (other is TileController tile) {
            Debug.Log("moving player to tile " + tile.CellNo);
            var path = GridSystem.Instance.PathFromAToB(
                GridSystem.Instance.CellNumber(transform.position),
                GridSystem.Instance.CellNumber(tile.transform.position)
            );

            if (path == null) {
                return true;
            }

            _pathToTake.Clear();
            foreach (Vector2 tilePos in path) {
                var cur = GridSystem.Instance.GetTileFromCellNumber(tilePos);
                var controller = cur.GetComponent<TileController>();
                controller.HighLightTile(true, Color.indianRed);
                _pathToTake.Add(controller);
            }

            _prevTileIdx = 0;
            _nextTileIdx = 1;
            _elapsedTime = 0;
        }

        return false;
    }
}
