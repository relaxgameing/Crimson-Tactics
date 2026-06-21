using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class TileManager : MonoBehaviour , IPointerEnterHandler,
IPointerExitHandler{

    private Outline _outlineComponent;
    private bool _isSelected = false;

    private void Start() {
        _outlineComponent = GetComponent<Outline>();
    }

    public void OnPointerEnter(PointerEventData eventData) {
        Debug.Log("Tile Selected " + GridGenerator.CellNumber(transform.position ));
        _outlineComponent.enabled = true;
    }
    public void OnPointerExit(PointerEventData eventData) {
        Debug.Log("Tile UnSelected");
        _outlineComponent.enabled = false;
    }
}
