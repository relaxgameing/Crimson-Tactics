using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class HUDUIBInder : MonoBehaviour {

    private UIDocument _ui;
    private Label _cellValue;
    private void OnEnable() {
        _ui = GetComponent<UIDocument>();
        if (_ui.IsUnityNull()) {
            Debug.LogError("Can not find UI Document for HUD binder component");
            return;
        }
        
        _cellValue = _ui.rootVisualElement.Query<Label>("cellNoValue").First();
    }

    private void Start() {
        _cellValue.dataSource = GameModeController.Instance;
    }
}
