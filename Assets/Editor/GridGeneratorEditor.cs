using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

[CustomEditor(typeof(GridSystem))]
public class GridGeneratorEditor : Editor {

    public VisualTreeAsset visualTreeAsset;
    private Button _clearGrid;
    private Button _generateGrid;
    private Button _loadGridFromScriptableObject;
    private Button _saveGridToScriptableObject;

    private GridSystem _gridSystem;

    private void OnEnable() {
        _gridSystem = (GridSystem)this.target;
    }

    public override VisualElement CreateInspectorGUI() {
        VisualElement root = new();

        InspectorElement.FillDefaultInspector(root, this.serializedObject, this);

        VisualElement custom = visualTreeAsset.Instantiate();
        root.Add(custom);

        _generateGrid = custom.Query<Button>("generateGrid");
        _clearGrid = custom.Query<Button>("clearGrid");
        _loadGridFromScriptableObject = custom.Query<Button>("loadGridFromScriptableObject");
        _saveGridToScriptableObject = custom.Query<Button>("saveGridToScriptableObject");

        _generateGrid.clicked += _gridSystem.Generate;
        _clearGrid.clicked +=  _gridSystem.Clear;
        _loadGridFromScriptableObject.clicked += _gridSystem.LoadGridFromScriptableObject;
        _saveGridToScriptableObject.clicked += _gridSystem.SaveGridToScriptableObject;
        return root;
    }
}
