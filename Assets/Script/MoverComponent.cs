using System;
using System.Collections.Generic;
using UnityEngine;

public enum MoverState {
    Idle, // idle , not moving
    Moving, // when it is moving
    Stopping, // state change from moving -> Idle
    Resetting // changing state to Fresh
}

public class MoverComponent : MonoBehaviour {
    [SerializeField] private float movementSpeed = 0.7f;
    [SerializeField] private float rotationSpeed = 0.2f;
    [SerializeField] private Vector3 lookAt = Vector3.forward;
    [SerializeField] private Color pathColor = Color.indianRed;
    [SerializeField] private bool isIndependentOfSimulationState = true;

    private List<TileController> _pathToTake;
    private List<Vector2> _newPath; // redirect to this path
    private bool _wantToRedirect;
    private int _nextTileIdx;
    private int _curTileIdx;
    private float _elapsedTime = 0;
    private IInteractable[] _interactables;

    public MoverState State = MoverState.Idle;

    public event Action OnPathChanged;

    private void Awake() {
        _pathToTake = new List<TileController>(10);
        _interactables = GetComponents<IInteractable>();
    }

    private void Update() {
        // we only move if the state is simulating
        Move();
    }

    public void ChangeMoverState(MoverState newState) {
        State = newState;

        // updates itself for the new state
        switch (State) {
            case MoverState.Idle:
                // path change routine
                SettingNewPath();
                break;
            case MoverState.Moving:
                break;
            case MoverState.Stopping:
                break;

            case MoverState.Resetting:
                _curTileIdx = 0;
                _nextTileIdx = 1;
                _elapsedTime = 0;

                foreach (TileController tile in _pathToTake) {
                    tile.HighLightTile(false, Color.white);
                }

                _pathToTake.Clear();

                ChangeMoverState(MoverState.Idle);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void SettingNewPath() {
        if (!_wantToRedirect) {
            return;
        }

        // ! order matters here we want to reset the _wantToRedirect
        // else it will go in recursion
        _wantToRedirect = false;
        SetPath(_newPath);
        _newPath = null;

        Debug.Log("Path changed , invoking");
        OnPathChanged?.Invoke();
    }

    private void Reset() {
        ChangeMoverState(MoverState.Resetting);
    }

    void SetPath(List<Vector2> path) {
        Reset();

        foreach (Vector2 tilePos in path) {
            var cur = GridSystem.Instance.GetTileFromCellNumber(tilePos);
            var controller = cur.GetComponent<TileController>();
            controller.HighLightTile(true, pathColor);
            _pathToTake.Add(controller);
        }
    }

    // this function is responsible for notifying the mover component that we are
    // changing the path we are currently taking , it doesnt update it instantly but
    // waits for the previous movement to next tile complete then change course
    public void ChangePath(List<Vector2> newPath) {
        _newPath = newPath;
        _wantToRedirect = true;
        StopMoving();
    }

    public void WhenPathChangeComplete(Action callback) {
        Action handle = null;
        handle = () => {
            OnPathChanged -= handle;
            Debug.Log("callback");
            callback();
        };

        OnPathChanged += handle;
    }

    public void StartMoving() {
        ChangeMoverState(MoverState.Moving);
    }

    // stop moving and reset itself
    void StopMoving() {
        ChangeMoverState(MoverState.Stopping);
    }

    public Vector2Int GetCellNumber() {
        if (_pathToTake.Count > 1) {
            return GridSystem.Instance.CellNumber(_pathToTake[_nextTileIdx].transform.position);
        }
        return GridSystem.Instance.CellNumber(transform.position);
    }

    void Move() {
        if (State == MoverState.Idle) {
            return;
        }

        if (_pathToTake.Count <= 1) {
            if (_wantToRedirect) {
                // so that we can change path
                ChangeMoverState(MoverState.Idle);
            }

            return;
        }

        var nextTile = _pathToTake[_nextTileIdx];
        var curTile = _pathToTake[_curTileIdx];
        if (!MoveOneTile(curTile, nextTile))
            return;

        Debug.Log("one tile movement completed");
        // when ever we reach a new tile we are interacting with it
        foreach (IInteractable interactable in _interactables) {
            nextTile.InteractWith(interactable);
        }

        lookAt = nextTile.transform.position - curTile.transform.position;
        _curTileIdx = _nextTileIdx;
        _nextTileIdx++;
        _elapsedTime = 0;


        if (_nextTileIdx >= _pathToTake.Count) {
            Debug.Log("path travel complete");
            ChangeMoverState(MoverState.Resetting);
            GameModeController.Instance.ObjectCompletedSimulating(gameObject);
        }

        if (State == MoverState.Stopping) {
            ChangeMoverState(MoverState.Idle);
        }
    }

    // returns true if moved completely
    bool MoveOneTile(TileController curTile, TileController nextTile) {
        _elapsedTime += Time.deltaTime;
        Vector3 movementDir = nextTile.transform.position - curTile.transform.position;

        float newRotation = Mathf.LerpAngle(
            Vector3.SignedAngle(Vector3.forward, lookAt, Vector3.up),
            Vector3.SignedAngle(Vector3.forward, movementDir, Vector3.up),
            _elapsedTime / rotationSpeed
        );

        Vector3 newPos = Vector3.Lerp(curTile.transform.position, nextTile.transform.position,
            _elapsedTime /
            movementSpeed);
        // we are not changing the y
        newPos.y = transform.position.y;

        // Debug.Log("updating transform");
        transform.SetPositionAndRotation(newPos, Quaternion.Euler(0, newRotation, 0));
        if (_elapsedTime >= movementSpeed) {
            return true;
        }

        return false;
    }
}
