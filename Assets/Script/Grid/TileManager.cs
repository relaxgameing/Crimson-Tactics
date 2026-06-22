using UnityEngine;
using UnityEngine.EventSystems;

public class TileManager : MonoBehaviour, IPointerEnterHandler,
    IPointerExitHandler {

    private Outline _outlineComponent;

    public Vector2 CellNo => GridSystem.CellNumber(this.transform.position);

    private void Start() {
        _outlineComponent = GetComponent<Outline>();
    }

    public void OnPointerEnter(PointerEventData eventData) {
        _outlineComponent.enabled = true;
        GameModeController.Instance.SetSelectedTile(this);
    }

    public void OnPointerExit(PointerEventData eventData) {
        _outlineComponent.enabled = false;
        if (GameModeController.Instance.SelectedTile == this) {
            GameModeController.Instance.SetSelectedTile(null);
        }
    }
}
