using System;
using Unity.VisualScripting;
using UnityEngine;

// we need mover to be able to move to target
[RequireComponent(typeof(MoverComponent))]
public class EnemyAI : MonoBehaviour, IInteractable {

    private MoverComponent _mover;
    private PlayerController _target;
    private Vector2Int _prevTile;

    private void Awake() {
        _mover = GetComponent<MoverComponent>();
    }

    public void SetTarget(PlayerController target) {
        _target = target;
    }

    private void Update() {
        if (_target.IsUnityNull())
            return;

        var targetCell = GridSystem.Instance.CellNumber(_target.transform.position);
        if (targetCell == _prevTile) {
            return;
        }

        var curPos = _mover.GetCellNumber();
        var newPath = GridSystem.Instance.PathFromAToB(
            curPos,
            targetCell,
            1,
            false
        );

        // when player reaches his destination then enemy is not moving because the target is now
        // considered as a obstacle
        if (newPath.IsUnityNull())
            return;

        Debug.Log("Changing ai path");
        _mover.ChangePath(newPath);
        _mover.WhenPathChangeComplete(_mover.StartMoving);
        _prevTile = targetCell;

    }

    public bool Interact() {
        return false;
    }

    public bool InteractWith(IInteractable other) {
        return false;
    }
}
