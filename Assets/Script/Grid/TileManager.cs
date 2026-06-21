using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class TileManager : MonoBehaviour , IPointerEnterHandler,
IPointerExitHandler{

    private Outline _outlineComponent;
    public Vector2 CellNo {
        get => GridGenerator.CellNumber(transform.position);
    }

    private void Start() {
        _outlineComponent = GetComponent<Outline>();
    }

    public void OnPointerEnter(PointerEventData eventData) {
        Debug.Log("Tile Selected " + GridGenerator.CellNumber(transform.position ));
        _outlineComponent.enabled = true;
        GameModeController.Instance.SetSelectedTile(this);
    }

    public void OnPointerExit(PointerEventData eventData) {
        Debug.Log("Tile UnSelected");
        _outlineComponent.enabled = false;
        if (GameModeController.Instance.SelectedTile == this) {
            GameModeController.Instance.SetSelectedTile(null);
        }
    }
}
