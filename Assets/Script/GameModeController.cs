using System;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.LowLevelPhysics2D;

public class GameModeController : MonoBehaviour {

    private TileManager _selectedTile;
    void Start()
    {
        Debug.Log("Game Started");
    }

    private void Update() {
        Vector2Control curPos = Mouse.current.position;
        Ray ray = Camera.main.ScreenPointToRay(curPos.ReadValue());
        if (Physics.Raycast(ray , out RaycastHit hit, 100 , LayerMask.GetMask("Tile"))) {
            GameObject obj = hit.collider.gameObject;
            obj.GetComponent<TileManager>().SetSelection(true);
            Debug.Log("hit " + obj.name);
        }
    }
}
