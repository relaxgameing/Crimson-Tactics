using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

[CustomEditor(typeof(GridSystem))]
public class GridGeneratorEditor : Editor {

    public VisualTreeAsset visualTreeAsset;
    private Button _clearGrid;
    private Button _generateGrid;

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

        _generateGrid.clicked += () => _gridSystem.Generate();
        _clearGrid.clicked += () => _gridSystem.Clear();

        return root;
    }
}
