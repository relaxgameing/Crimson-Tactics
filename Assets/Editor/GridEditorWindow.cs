using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;


public class GridEditorWindow : EditorWindow {
    // the asset selected by the player
    private ObjectField _assetSelectedForPlacement;
    private Button _confirmPlacement; // doesnt do anything till now
    // can be selected by the player to allow placements on grid with this specific tag
    private TagField _gridTag;

    // the grid system which will be modified
    private GridSystem _gridSystem;
    // is the player currently editing the grid
    private bool _isEditing = false;

    private GameObject _placementAsset;
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
        _confirmPlacement = root.Query<Button>("confirmPlacement").First();
        _assetSelectedForPlacement = root.Query<ObjectField>("selectedPlacementAsset").First();
        _gridTag = root.Query<TagField>("targetGridTag").First();

        _assetSelectedForPlacement.RegisterValueChangedCallback(HandleObstacleValueChange);
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
        if (cur.IsUnityNull() || !cur.CompareTag(_gridTag.value)) {
            return;
        }

    }

    // not implemented yet
    private void HandleAssetPlacement() { }

    private void HandleStartEditing() {
        _isEditing = !_isEditing;

        if (_isEditing) {
            _confirmPlacement.visible = true;
            _startEditingBtn.text = "Stop Editing";

            var dimension= _gridSystem.GridDimension;
            ChangeSceneView(new Vector3( dimension.x / 2 , 8 , dimension.y / 2) ,
                Quaternion.Euler(90 , 0 , 0) );
        }
        else {
            _confirmPlacement.visible = false;
            _startEditingBtn.text = "Start Editing";

            var dimension= _gridSystem.GridDimension;
            // the angle values and height is taken from main camera position
            ChangeSceneView(new Vector3( dimension.x  , 3 , dimension.y) ,
                Quaternion.Euler( 30, 225 , 0) );
        }
    }

    private void ChangeSceneView(Vector3 pos, Quaternion dir) {
            var sv = SceneView.lastActiveSceneView;
            if (!sv) {
                return;
            }
            sv.LookAt(pos,
                dir,
                1);
            sv.Repaint();
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
