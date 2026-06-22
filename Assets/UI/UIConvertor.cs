using Unity.VisualScripting;
using UnityEditor;
using UnityEngine.UIElements;

public static class UIConvertor {

    [InitializeOnLoadMethod]
    public static void RegisterEditorConverters() {
        ConverterGroup group = new("TileManagerConverters");
        group.AddConverter<TileManager, string>(CellNoConvertor);
        ConverterGroups.RegisterConverterGroup(group);
    }

    public static string CellNoConvertor(ref TileManager tile) {
        if (tile.IsUnityNull()) {
            return " No Selection";
        }

        return $"( {tile.CellNo.x},{tile.CellNo.y} )";
    }
}
