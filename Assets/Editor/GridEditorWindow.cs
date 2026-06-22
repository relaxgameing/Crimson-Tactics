using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;


public class GridEditorWindow : EditorWindow {

    private GridSystem _gridSystem;
    private bool _isEditing;

    private GameObject _obstacle;
    private ObjectField _selectedObstacle;
    private Button _startEditingBtn;

    private void OnEnable() {
        _gridSystem = FindAnyObjectByType<GridSystem>();
        if (_gridSystem.IsUnityNull()) {
            Warning.Info("No GridSystem GameObject Found in Scene");
        }
    }

    private void CreateGUI() {
        VisualElement root = this.rootVisualElement;

        VisualTreeAsset visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
            "Assets/UI Toolkit/Editor_GridEditor.uxml"
        );

        visualTree.CloneTree(root);

        _startEditingBtn = root.Query<Button>("startEditingGrid").First();
        _selectedObstacle = root.Query<ObjectField>("selectedObstacleAsset").First();

        _selectedObstacle.RegisterValueChangedCallback(HandleObstacleValueChange);
        _startEditingBtn.clicked += HandleStartEditing;
    }

    private void OnSelectionChange() { }

    private void HandleStartEditing() {
        _isEditing = !_isEditing;

        _startEditingBtn.text = _isEditing ? "Stop Editing" : "Start Editing";
    }

    private void HandleObstacleValueChange(ChangeEvent<Object> evt) {
        if (evt.newValue == null) {
            _obstacle = null;
            return;
        }

        GameObject val = evt.newValue as GameObject;
        NormalizeToGridUnit.NormalizeToOneUnit(val);
        _obstacle = val;
    }

    [MenuItem("GridTools/Grid Editor")]
    public static void ShowWindow() {
        GridEditorWindow window = GetWindow<GridEditorWindow>();
        window.titleContent = new GUIContent("Grid Editor Window");
        window.Show();
    }
}
