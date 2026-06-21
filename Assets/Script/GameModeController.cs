using UnityEngine;

public class GameModeController : MonoBehaviour {
    private static GameModeController _instance;

    // Lazy initialization
    public static GameModeController Instance {
        get {
            if (_instance is null) {
                // Try to find existing instance in scene
                _instance = FindObjectOfType<GameModeController>();

                // If still not found, create a new GameObject
                if (_instance is null) {
                    GameObject obj = new GameObject("GameModeController");
                    _instance = obj.AddComponent<GameModeController>();
                    DontDestroyOnLoad(obj);
                }
            }
            return _instance;
        }
    }

    private TileManager _selectedTile;
    void Start()
    {
        Debug.Log("Game Started");
    }

}
