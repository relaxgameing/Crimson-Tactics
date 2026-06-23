using System;
using System.Collections.Generic;
using UnityEngine;

public class MoverComponent : MonoBehaviour {
    [SerializeField] private float movementSpeed = 0.7f;
    [SerializeField] private float rotationSpeed = 0.2f;
    [SerializeField] private Vector3 lookAt = Vector3.forward;

    private List<TileController> _pathToTake;
    private int _nextTileIdx;
    private int _prevTileIdx;
    private float _elapsedTime = 0;
    private bool _canMove = false;

    private void Awake() {
        _pathToTake = new List<TileController>(10);
    }

    private void Update() {
        // we only move if the state is simulating
        if (_canMove && GameModeController.Instance.GameState == GameState.Simulating) {
            Move();
        }
    }

    private void Reset() {
        _prevTileIdx = 0;
        _nextTileIdx = 1;
        _elapsedTime = 0;
        _pathToTake.Clear();
        _canMove = false;
    }

    public void SetPath(List<Vector2> path) {
        Reset();

        foreach (Vector2 tilePos in path) {
            var cur = GridSystem.Instance.GetTileFromCellNumber(tilePos);
            var controller = cur.GetComponent<TileController>();
            controller.HighLightTile(true, Color.indianRed);
            _pathToTake.Add(controller);
        }
    }

    public void StartMoving() {
        _canMove = true;
    }

     void Move() {
        if (_pathToTake.Count <= 1) return;

        _elapsedTime += Time.deltaTime;

        var nextTile = _pathToTake[_nextTileIdx];
        var prevTile = _pathToTake[_prevTileIdx];
        Vector3 movementDir = nextTile.transform.position - prevTile.transform.position;

        float newRotation = Mathf.LerpAngle(
            Vector3.SignedAngle(Vector3.forward, lookAt, Vector3.up),
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
            lookAt = nextTile.transform.position - prevTile.transform.position;
            _prevTileIdx = _nextTileIdx;
            _nextTileIdx++;
            _elapsedTime = 0;
        }

        if (_nextTileIdx >= _pathToTake.Count) {
            Debug.Log("path travel complete");
            foreach (TileController tile in _pathToTake) {
                tile.HighLightTile(false, Color.white);
            }

            Reset();
            GameModeController.Instance.ObjectCompletedSimulating(gameObject);
        }
    }
}
