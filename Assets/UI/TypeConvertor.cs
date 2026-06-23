using System;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;
using UnityEngine.UIElements;

public static class TypeConvertor {

    [InitializeOnLoadMethod]
    public static void RegisterEditorConverters() {
        ConverterGroup group = new("TypeConvertors");
        group.AddConverter<TileController, string>(CellNoConvertor);
        group.AddConverter<GameState, string>(GameStateToString);
        ConverterGroups.RegisterConverterGroup(group);
    }

    public static string CellNoConvertor(ref TileController tile) {
        if (tile.IsUnityNull()) {
            return " No Selection";
        }

        return $"( {tile.CellNo.x},{tile.CellNo.y} )";
    }

    public static Vector2Int Vector2ToVector2Int(Vector2 a) {
        return new Vector2Int((int)a.x, (int)a.y);
    }

    public static string GameStateToString(ref GameState state) {
        return Enum.GetName(typeof(GameState) , state);
    }
}
