using Unity.VisualScripting;
using Unity.VisualScripting.YamlDotNet.Core.Tokens;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;


public class GridEditorWindow : EditorWindow {

    private GridSystem _gridSystem;
    private bool _isEditing;

    private GameObject _placementAsset;
    private ObjectField _assetToPlace;
    private TagField _targetGridTag;
    private Button _startEditingBtn;
    private Button _confirmPlacement;

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
        _confirmPlacement = root.Query<Button>("confirmPlacement").First();
        _assetToPlace = root.Query<ObjectField>("selectedPlacementAsset").First();
        _targetGridTag = root.Query<TagField>("targetGridTag").First();

        _assetToPlace.RegisterValueChangedCallback(HandleObstacleValueChange);
        _startEditingBtn.clicked += HandleStartEditing;
        _confirmPlacement.visible = false;
    }

    private void OnSelectionChange() { }

    private void HandleStartEditing() {
        _isEditing = !_isEditing;

        if (_isEditing) {
            _confirmPlacement.visible = true;
            _startEditingBtn.text = "Stop Editing";
        }else {
            _confirmPlacement.visible = false;
            _startEditingBtn.text = "Start Editing";
        }
    }

    private void HandleObstacleValueChange(ChangeEvent<Object> evt) {
        if (evt.newValue == null) {
            _placementAsset= null;
            return;
        }

        GameObject val = evt.newValue as GameObject;
        NormalizeToGridUnit.NormalizeToOneUnit(val);
        _placementAsset= val;
    }

    [MenuItem("GridTools/Grid Editor")]
    public static void ShowWindow() {
        GridEditorWindow window = GetWindow<GridEditorWindow>();
        window.titleContent = new GUIContent("Grid Editor Window");
        window.Show();
    }
}
