using System;
using System.ComponentModel;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class TileController : MonoBehaviour, IPointerEnterHandler,
    IPointerExitHandler, IInteractable {
    [SerializeField] private GameObject objectOnTile;
    private GameObject _objectOnTileInstance;
    private Outline _outlineComponent;

    private bool _isHighLighted = false;

    public bool IsOccupied => objectOnTile.transform.childCount > 0;

    public Vector2 CellNo => GridSystem.Instance.CellNumber(this.transform.position);

    private void OnEnable() {
        _outlineComponent = GetComponent<Outline>();
    }

    public void OnPointerEnter(PointerEventData eventData) {
        _outlineComponent.enabled = true;
        GameModeController.Instance.SetSelectedTile(this);
    }

    public void OnPointerExit(PointerEventData eventData) {
        if (!_isHighLighted) {
            _outlineComponent.enabled = false;
        }
        if (GameModeController.Instance.SelectedTile == this) {
            GameModeController.Instance.SetSelectedTile(null);
        }
    }

    public void HighLightTile(bool val , Color color ) {
        _isHighLighted = val;
        _outlineComponent.enabled = val;
        _outlineComponent.OutlineColor = color;
    }

    public void SetObjectOnTile(GameObject obj) {
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

        return false;
    }
}
