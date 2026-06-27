using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Properties;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;



public class GridEditorWindow : EditorWindow {
    // the asset selected by the player
    private ObjectField _assetSelectedForPlacement;
    // can be selected by the player to allow placements on grid with this specific tag
    private TagField _gridTag;

    // section which should only be visible when editing
    private VisualElement _editingSection;
    private EnumField _opMode;
    private Button _confirmPlacement; // doesnt do anything till now
    private Button _discardPlacement;

    // the grid system which will be modified
    private GridSystem _gridSystem;
    // is the player currently editing the grid
    private bool _isEditing = false;

    private GameObject _placementAsset;
    private Button _startEditingBtn;

    // current changes made to the scene
    private Dictionary<Vector2Int, Stack<ObstaclesChange>> _changes = new();

    [CreateProperty] public GridOperation curMode = GridOperation.Adding;

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
        _opMode = root.Query<EnumField>("operationMode").First();
        _editingSection = root.Query<VisualElement>("editingSection").First();
        _discardPlacement = root.Query<Button>("discardChanges").First();


        _assetSelectedForPlacement.RegisterValueChangedCallback(HandleObstacleValueChange);
        _startEditingBtn.clicked += HandleStartEditing;

        _confirmPlacement.clicked += HandleConfirmPlacement;
        _discardPlacement.clicked += HandleDiscardPlacement;

        _opMode.dataSource = this;
    }




    private void OnSelectionChange() {
        Debug.Log($"{curMode} assets on tile");
        if (!_isEditing) {
            return;
        }

        // if an gameobject is selected currently
        GameObject tileSelected = Selection.activeGameObject;
        if (tileSelected.IsUnityNull()) {
            return;
        }

        // get information about it
        var curCellNo = _gridSystem.CellNumber(tileSelected.transform.position);
        Debug.Log($"selected number {curCellNo}");
        var curTileInfo = _gridSystem.GetTileInfoOfCellNumber(curCellNo);
        // means tile is not valid
        if (curTileInfo == null || curTileInfo.Tile == null || !curTileInfo.Tile.CompareTag(_gridTag
                .value)) {
            return;
        }

        if (!_changes.ContainsKey(curCellNo)) {
            _changes[curCellNo] = new();
        }

        var lastChange = _changes[curCellNo].Count > 0 ? _changes[curCellNo].Peek() : null;
        switch (curMode) {
            case GridOperation.Adding:
                if (_placementAsset == null) {
                    EditorUtility.DisplayDialog("Operation Invalid", "Select a prefab before " +
                        "editing grid", "understood");
                    break;
                }

                // we are checking the grid and also the changes we have made so far but not yet
                // commited
                if (curTileInfo.isOccupied || (lastChange != null && lastChange.op == GridOperation
                        .Adding)) {
                    Debug.Log("Cannot place asset , tile is occupied remove the asset first");
                    break;
                }

                var prefabInstance = (GameObject)PrefabUtility.InstantiatePrefab(_placementAsset,
                    tileSelected.transform);

                if (prefabInstance == null) {
                    Debug.Log($"Cannot instansiate prefab {_placementAsset.name}");
                    break;
                }

                prefabInstance.tag = TagHandle.GetExistingTag("Obstacle").ToString();

                AddObstacleChangeAt(curCellNo, new ObstaclesChange(
                    _placementAsset,
                    prefabInstance,
                    curMode
                ));


                break;
            case GridOperation.Removing:
                // tile should have an obstacle and the last operation perform we have added an
                // obstacle
                if (!curTileInfo.isOccupied &&
                    (lastChange != null && lastChange.op == GridOperation.Removing)) {
                    Debug.Log("No Obstacle to Remove");
                    break;
                }

                // this means we are removing save obstacle
                if (lastChange == null) {
                    foreach (GameObject ob in curTileInfo.Obstacles) {
                        // means already removed temporarily
                        if (!ob.activeInHierarchy || !ob.CompareTag("Obstacle")) {
                            continue;
                        }

                        ob.SetActive(false);
                        AddObstacleChangeAt(curCellNo, new ObstaclesChange(
                            GridData.GetPrefab(ob),
                            ob,
                            curMode
                        ));
                    }

                    // we can skip this last part for case where we are removing saved obstacle
                    break;
                }


                _changes[curCellNo].Pop();
#if UNITY_EDITOR
                DestroyImmediate(lastChange.instance);
#else
                Destroy(lastAddOp.instance);
#endif
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void AddObstacleChangeAt(Vector2Int cellNo, ObstaclesChange change) {
        if (_changes[cellNo] == null) {
            _changes[cellNo] = new();
        }

        _changes[cellNo].Push(change);
    }

    private void HandleConfirmPlacement() {
        foreach (var changes in _changes) {
            Debug.Log($"Confirming Changes for cell {changes.Key}");
            _gridSystem.UpdateGridChanges(changes.Key, changes.Value);
        }
        _gridSystem.ValidateGrid();
        _gridSystem.SaveGridToScriptableObject();
        _changes.Clear();
    }

    private void HandleDiscardPlacement() {
        _gridSystem.LoadGridFromScriptableObject();
        _changes.Clear();
    }

    private void HandleStartEditing() {
        _isEditing = !_isEditing;

        if (_isEditing) {
            _startEditingBtn.text = "Stop Editing";
            _editingSection.visible = true;

            var dimension = _gridSystem.GridDimension;
            ChangeSceneView(new Vector3(dimension.x / 2, 8, dimension.y / 2),
                Quaternion.Euler(90, 0, 90));
        }
        else {
            _editingSection.visible = false;
            _startEditingBtn.text = "Start Editing";

            var dimension = _gridSystem.GridDimension;
            // the angle values and height is taken from main camera position
            ChangeSceneView(new Vector3(dimension.x, 3, dimension.y),
                Quaternion.Euler(30, 225, 0));
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
        if (!PrefabUtility.IsPartOfPrefabAsset(val)) {
            EditorUtility.DisplayDialog("Not Valid Prefab", "Selected object is not a active " +
                                                            "prefab instance", "change prefab");
            _assetSelectedForPlacement.value = null;
            return;
        }

        // NormalizeToGridUnit.NormalizeToOneUnit(val);
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
