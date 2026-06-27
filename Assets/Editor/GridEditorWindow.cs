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
    private Button _confirmPlacement; // doesnt do anything till now
    // can be selected by the player to allow placements on grid with this specific tag
    private TagField _gridTag;
    private EnumField _opMode;

    // the grid system which will be modified
    private GridSystem _gridSystem;
    // is the player currently editing the grid
    private bool _isEditing = false;

    private GameObject _placementAsset;
    private Button _startEditingBtn;

    // current changes made to the scene
    private Dictionary<Vector2Int, List<ObstaclesChange>> _changes = new();

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

        _assetSelectedForPlacement.RegisterValueChangedCallback(HandleObstacleValueChange);
        _startEditingBtn.clicked += HandleStartEditing;

        _confirmPlacement.clicked += HandleConfirmPlacement;
        _confirmPlacement.visible = false;


        _opMode.dataSource = this;
    }


    private void OnSelectionChange() {
        Debug.Log($"{curMode} assets on tile");
        if (!_isEditing) {
            return;
        }


        GameObject tileSelected = Selection.activeGameObject;
        if (tileSelected.IsUnityNull()) {
            return;
        }

        var cellNo = _gridSystem.CellNumber(tileSelected.transform.position);
        var tileInfo = _gridSystem.GetTileInfoOfCellNumber(cellNo);
        // means tile is not valid
        if (tileInfo == null) {
            return;
        }

        if (!_changes.ContainsKey(cellNo)) {
            _changes[cellNo] = new();
        }


        switch (curMode) {
            case GridOperation.Adding:
                if (_placementAsset == null) {
                    EditorUtility.DisplayDialog("Operation Invalid", "Select a prefab before " +
                        "editing grid" , "understood");
                    break;
                }
                // we are checking the grid and also the changes we have made so far but not yet
                // commited
                if (tileInfo.isOccupied ||
                    _changes[cellNo].Find(val => val.op == GridOperation
                        .Adding) != null) {
                    Debug.Log("Cannot place asset , tile is occupied remove the asset first");
                    break;
                }

                var prefabInstance = (GameObject)PrefabUtility.InstantiatePrefab(_placementAsset,
                    tileSelected.transform);

                if (prefabInstance == null) {
                    Debug.Log($"Cannot instansiate prefab {_placementAsset.name}");
                    break;
                }

                AddObstacleChangeAt(cellNo, new ObstaclesChange(
                    _placementAsset,
                    prefabInstance,
                    curMode
                ));


                break;
            case GridOperation.Removing:

                if (!tileInfo.isOccupied && _changes[cellNo].Find(val => val.op == GridOperation
                        .Adding) == null) {
                    Debug.Log("No Obstacle to Remove");
                    break;
                }

                foreach (GameObject ob in tileInfo.Obstacles) {
                    // means already removed temporarily
                    if (!ob.activeInHierarchy) {
                        continue;
                    }

                    ob.SetActive(false);
                    AddObstacleChangeAt(cellNo, new ObstaclesChange(
                        GridData.GetPrefab(ob),
                        ob,
                        curMode
                    ));
                }

                // removing the last add operation
                var lastAddOp = _changes[cellNo].FindLast(val => val.op == GridOperation
                    .Adding);
                _changes[cellNo].Remove(lastAddOp);

#if UNITY_EDITOR
                DestroyImmediate(lastAddOp.instance);
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

        _changes[cellNo].Add(change);
    }

    private void RemoveLastObstacleChangeAt(Vector2Int cellNo) {
        var changes = _changes[cellNo];
        if (changes == null || changes.Count == 0) {
            return;
        }

        changes.RemoveAt(changes.Count - 1);
    }


    private void RemoveObstacleChangeAt(Vector2Int cellNo, ObstaclesChange change) {
        var changes = _changes[cellNo];
        if (changes == null) {
            return;
        }

        int idx = _changes[cellNo].FindIndex(
            (obstaclesChange =>
                obstaclesChange.instance == change.instance &&
                obstaclesChange.op == change.op));

        if (idx == -1) {
            return;
        }

        _changes[cellNo].RemoveAt(idx);
    }

    private void HandleConfirmPlacement() {
        foreach (var changes in _changes) {
            Debug.Log($"Confirming Changes for cell {changes.Key}");
            _gridSystem.UpdateGridChanges(changes.Key, changes.Value);
        }

        _gridSystem.ValidateGrid();
    }

    private void HandleStartEditing() {
        _isEditing = !_isEditing;

        if (_isEditing) {
            _confirmPlacement.visible = true;
            _startEditingBtn.text = "Stop Editing";

            var dimension = _gridSystem.GridDimension;
            ChangeSceneView(new Vector3(dimension.x / 2, 8, dimension.y / 2),
                Quaternion.Euler(90, 0, 90));
        }
        else {
            _confirmPlacement.visible = false;
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
        if ( !PrefabUtility.IsPartOfPrefabAsset(val)) {
            EditorUtility.DisplayDialog("Not Valid Prefab" , "Selected object is not a active " +
                "prefab instance" , "change prefab");
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
