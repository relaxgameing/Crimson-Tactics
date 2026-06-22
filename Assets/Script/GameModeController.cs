using System;
using Unity.Properties;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public enum GameState {
    GameStart, // when game starts
    Simulating, // when any event is occuring
    Idle // when player is deciding a move
}

public class GameModeController : MonoBehaviour {
    private static GameModeController _instance;
    [CreateProperty] public TileController SelectedTile { get; private set; }
    [SerializeField] private PlayerController _player;
    [SerializeField] private InputActionReference interactionAction;

    public GameState GameState { get; private set; }

    public Action<GameState> OnGameStateChange;

    // Lazy initialization
    public static GameModeController Instance {
        get {
            if (_instance is null) {
                // Try to find existing instance in scene
                _instance = FindObjectOfType<GameModeController>();

                // If still not found, create a new GameObject
                if (_instance is null) {
                    GameObject obj = new("GameModeController");
                    _instance = obj.AddComponent<GameModeController>();
                    DontDestroyOnLoad(obj);
                }
            }

            return _instance;
        }
    }

    void HandleGameStateChange(GameState newState) {
        switch (newState) {
            case GameState.GameStart:
                // do something then change the state to idle
                ChangeGameState(GameState.Idle);
                break;
            case GameState.Simulating:
                break;
            case GameState.Idle:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(newState), newState, null);
        }
    }

    private void OnEnable() {
        interactionAction.ToInputAction().performed += HandleInteraction;
    }

    private void OnDisable() {
        interactionAction.ToInputAction().performed -= HandleInteraction;
    }

    private void HandleInteraction(InputAction.CallbackContext obj) {
        if (SelectedTile != null) {
            SelectedTile.InteractWith(_player);
            _player.InteractWith(SelectedTile);
        }
        
        Debug.Log("clicked");
    }

    private void Start() {
        Debug.Log("Game Started");
        ChangeGameState(GameState.GameStart);
    }

    public void SetSelectedTile(TileController newTile) {
        SelectedTile = newTile;
    }

    public void ChangeGameState(GameState newState) {
        GameState = newState;

        // we want the gameModeController to get the state update first
        HandleGameStateChange(GameState);
        OnGameStateChange?.Invoke(GameState);
    }

}
