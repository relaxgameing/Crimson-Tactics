using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class HUDUIBInder : MonoBehaviour {
    private Label _cellValue;
    private Label _stateValue;

    private UIDocument _ui;

    private void Start() {
        _cellValue.dataSource = GameModeController.Instance;
        _stateValue.dataSource = GameModeController.Instance;
    }

    private void OnEnable() {
        _ui = GetComponent<UIDocument>();
        if (_ui.IsUnityNull()) {
            Debug.LogError("Can not find UI Document for HUD binder component");
            return;
        }

        _cellValue = _ui.rootVisualElement.Query<Label>("cellNoValue").First();
        _stateValue = _ui.rootVisualElement.Query<Label>("stateValue").First();
    }
}
