using System;
using System.Collections.Generic;
using System.ComponentModel;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class TileController : MonoBehaviour, IPointerEnterHandler,
    IPointerExitHandler, IInteractable {
    [SerializeField] private GameObject objectOnTile;
    [SerializeField] private Color pointerColor = Color.white;
    private GameObject _objectOnTileInstance;
    private Outline _outlineComponent;
    public bool IsOccupied => objectOnTile.transform.childCount > 0;

    public Vector2 CellNo => GridSystem.Instance.CellNumber(this.transform.position);

    // the number of entity has this tile on focus
    private HashSet<int> _InFocus;
    private Color? _prevColor = null;
    private int _pointerID = Int32.MaxValue;

    private void Awake() {
        _InFocus = new HashSet<int>();
    }

    private void OnEnable() {
        _outlineComponent = GetComponent<Outline>();
    }

    public void OnPointerEnter(PointerEventData eventData) {
        AddFocus(_pointerID , pointerColor);
        GameModeController.Instance.SetSelectedTile(this);
    }

    public void OnPointerExit(PointerEventData eventData) {
        RemoveFocus(_pointerID);
        if (GameModeController.Instance.SelectedTile == this) {
            GameModeController.Instance.SetSelectedTile(null);
        }
    }

    public void AddFocus(int instanceId, Color color) {
        if (instanceId != _pointerID) {
            _prevColor = color;
        }
        _InFocus.Add(instanceId);
        HighLightTile(true, color);
    }

    public void RemoveFocus(int instanceId) {
        _InFocus.Remove(instanceId);
        if (instanceId == _pointerID) {
            _outlineComponent.OutlineColor = _prevColor ?? pointerColor;
        }

        if (_InFocus.Count == 0) {
            HighLightTile(false , pointerColor);
        }
    }

    public void HighLightTile(bool val, Color color) {
        _outlineComponent.enabled = val;
        _outlineComponent.OutlineColor = color;
    }

    public void PlaceObjectOnTile(GameObject obj) {
        if (!_objectOnTileInstance.IsUnityNull()) {
#if UNITY_EDITOR
            DestroyImmediate(_objectOnTileInstance);
#else
            Destroy(_objectOnTileInstance);
#endif
        }

        _objectOnTileInstance = Instantiate(obj, objectOnTile.transform);
    }

    public bool Interact() {
        return true;
    }

    public bool InteractWith(IInteractable other) {
        if (other is PlayerController player) {
            Debug.Log("Interaction of tile with " + player.name);
            player.transform.SetParent(objectOnTile.transform);
            return true;
        }

        if (other is EnemyAIController ai) {
            Debug.Log("Interaction of tile with " + ai.name);
            ai.transform.SetParent(objectOnTile.transform);
            return true;
        }

        return false;
    }
}
