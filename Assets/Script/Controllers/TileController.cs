using System;
using System.Collections.Generic;
using System.ComponentModel;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class TileController : MonoBehaviour, IPointerEnterHandler,
    IPointerExitHandler, IInteractable {
    [SerializeField] private Color pointerColor = Color.white;
    private Outline _outlineComponent;

    public Vector2Int CellNo => GridSystem.Instance.CellNumber(this.transform.position);

    // the number of entity has this tile on focus
    private HashSet<int> _InFocus;
    private Color? _prevColor = null;
    // used for identifying cursor related events
    private readonly int _pointerID = Int32.MinValue;

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

    public bool Interact() {
        return true;
    }

    public bool InteractWith(IInteractable other) {
        if (other is PlayerController player) {
            Debug.Log("Interaction of tile with " + player.name);
            return true;
        }

        if (other is EnemyAIController ai) {
            Debug.Log("Interaction of tile with " + ai.name);
            return true;
        }

        return false;
    }
}
