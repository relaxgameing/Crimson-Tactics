using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class TileController : MonoBehaviour, IPointerEnterHandler,
    IPointerExitHandler {
    [SerializeField] private GameObject objectOnTile;
    private GameObject _objectOnTileInstance;
    private Outline _outlineComponent;

    public bool IsOccupied => !_objectOnTileInstance.IsUnityNull();

    public Vector2 CellNo => GridSystem.CellNumber(this.transform.position);

    private void OnEnable() {
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
}
