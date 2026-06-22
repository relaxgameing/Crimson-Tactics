using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;


public class GridEditorWindow : EditorWindow {
    private ObjectField _assetToPlace;
    private Button _confirmPlacement;

    private GridSystem _gridSystem;
    private bool _isEditing = false;

    private GameObject _placementAsset;
    private Button _startEditingBtn;
    private TagField _targetGridTag;

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

        _confirmPlacement.clicked += HandleAssetPlacement;
        _confirmPlacement.visible = false;
    }

    private void OnSelectionChange() {
        Debug.Log("placing assets on tile");
        if (!_isEditing) {
            return;
        }

        GameObject cur = Selection.activeGameObject;
        if (cur.IsUnityNull() || !cur.CompareTag(_targetGridTag.value)) {
            return;
        }

        TileController tileController = cur.GetComponent<TileController>();
        tileController.SetObjectOnTile(_placementAsset);
    }

    private void HandleAssetPlacement() { }

    private void HandleStartEditing() {
        _isEditing = !_isEditing;

        if (_isEditing) {
            _confirmPlacement.visible = true;
            _startEditingBtn.text = "Stop Editing";

            var _cam =SceneView.GetAllSceneCameras()[0];
            var dimension= _gridSystem.GridDimension;
            var pos = _cam.transform.position;
            _cam.transform.SetPositionAndRotation(new Vector3(pos.x + dimension.x / 2 ,
                10 ,
                pos.z + dimension.y / 2 ) ,
                Quaternion.Euler(0 , 90 , 0));
        }
        else {
            _confirmPlacement.visible = false;
            _startEditingBtn.text = "Start Editing";
        }
    }

    private void HandleObstacleValueChange(ChangeEvent<Object> evt) {
        if (evt.newValue == null) {
            _placementAsset = null;
            return;
        }

        GameObject val = (GameObject)evt.newValue;
        NormalizeToGridUnit.NormalizeToOneUnit(val);
        _placementAsset = val;
        Debug.Log("Setting " + val.name + " as placement asset");
    }

    [MenuItem("GridTools/Grid Editor")]
    public static void ShowWindow() {
        GridEditorWindow window = GetWindow<GridEditorWindow>();
        window.titleContent = new GUIContent("Grid Editor Window");
        window.Show();
    }
}
