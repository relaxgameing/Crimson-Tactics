using System;
using System.Collections.Generic;
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
    [SerializeField] private InputActionReference interactionAction;
    [CreateProperty] public GameState GameState { get; private set; }
    private PlayerController _player;
    private HashSet<GameObject> _currentlySimulating;

    public Action<GameState> OnGameStateChange;
    public Action OnSimulationComplete;

    // Lazy initialization
    public static GameModeController Instance {
        get {
            if (_instance is null) {
                // Try to find existing instance in scene
                _instance = FindAnyObjectByType<GameModeController>();

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
                var enemies = FindObjectsByType<EnemyAIController>(FindObjectsSortMode.None);
                foreach (EnemyAIController ai in enemies) {
                    ai.SetTarget(_player);
                }
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

    private void Awake() {
        _currentlySimulating = new HashSet<GameObject>();
    }

    private void OnEnable() {
        _player = FindAnyObjectByType<PlayerController>();
        interactionAction.ToInputAction().performed += HandleInteraction;
    }

    private void OnDisable() {
        interactionAction.ToInputAction().performed -= HandleInteraction;
    }

    // this is the entry point of all kind of interaction with the game
    private void HandleInteraction(InputAction.CallbackContext obj) {
        // no interactions will occur when the state is in simulating
        if (GameState == GameState.Simulating) {
            return;
        }

        if (SelectedTile != null) {
            _player.InteractWith(SelectedTile);

            // here order matters because SelectedTile is relocating the player parent
            SelectedTile.InteractWith(_player);
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

    public void AddObjectSimulating(GameObject obj) {
        _currentlySimulating.Add(obj);
        if (_currentlySimulating.Count > 0 && GameState != GameState.Simulating) {
            ChangeGameState(GameState.Simulating);
        }
    }

    public void ObjectCompletedSimulating(GameObject obj) {
        _currentlySimulating.Remove(obj);
        if (_currentlySimulating.Count == 0  && GameState == GameState.Simulating) {
            ChangeGameState(GameState.Idle);
        }
        OnSimulationComplete?.Invoke();
    }
}
