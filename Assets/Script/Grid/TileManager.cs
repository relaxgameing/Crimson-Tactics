using System;
using UnityEngine;

public class TileManager : MonoBehaviour {

    private Outline _outlineComponent;
    private bool _isSelected = false;

    private void Start() {
        _outlineComponent = GetComponent<Outline>();
    }

    private void Selected() {
        Debug.Log("Mouse Enter");
        var pos = transform.position;
        pos.y += 0.2f;
        transform.SetPositionAndRotation(pos , transform.rotation);
        _outlineComponent.enabled = true;
    }

    private void NotSelected() {
        Debug.Log("Mouse Exit");
        var pos = transform.position;
        pos.y -= 0.2f;
        transform.SetPositionAndRotation(pos , transform.rotation);
        _outlineComponent.enabled = false;
    }

    public void SetSelection(bool isSelected) {
        _isSelected = isSelected;

        if (isSelected) {
            Selected();
        }else {
            NotSelected();
        }
    }
}
