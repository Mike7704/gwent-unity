using System;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Manages game state and overall game flow.
/// </summary>
public class GameManager : Singleton<GameManager>
{
    // Events for state changes to allow other systems to respond
    public static event Action<GameState> OnBeforeStateChanged;
    public static event Action<GameState> OnAfterStateChanged;

    public GameState State { get; private set; } = GameState.None;

    private bool systemsInitialised = false;

    void Start()
    {
        Debug.Log("[GameManager] Starting GameManager...");
        ChangeState(GameState.Starting);
    }

    /// <summary>
    /// Handles state transitions and notifies listeners.
    /// </summary>
    public void ChangeState(GameState newState)
    {
        if (State == newState) return; // Prevent duplicate state changes

        Debug.Log($"[GameManager] Changing state: {newState}");
        OnBeforeStateChanged?.Invoke(newState);

        State = newState;
        switch(newState) {
            case GameState.Starting:
                HandleStarting();
                break;
            case GameState.MainMenu:
                HandleMainMenu();
                break;
            case GameState.DeckMenu:
                HandleDeckMenu();
                break;
            case GameState.SettingsMenu:
                HandleSettingsMenu();
                break;
            case GameState.SinglePlayer:
                HandleSinglePlayer();
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(newState), newState, null);
        }

        OnAfterStateChanged?.Invoke(newState);
    }

    // -------------------------
    // State Handlers
    // -------------------------

    private void HandleStarting()
    {
        Debug.Log("[GameManager] Initialising Systems...");

        if (!systemsInitialised)
        {
            AudioSystem.Instance.Initialise();
            CardDatabase.Instance.LoadAllCards();  // Preload JSON & cache
            SettingsManager.Instance.LoadSettings();
            DeckManager.Instance.LoadDeck();
            systemsInitialised = true;
        }

        // Transition to MainMenu after setup
        ChangeState(GameState.MainMenu);
    }

    private void HandleMainMenu()
    {
        LoadScene("MainMenu");
    }

    private void HandleDeckMenu()
    {
        LoadScene("DeckMenu");
    }

    private void HandleSettingsMenu()
    {
        LoadScene("SettingsMenu");
    }

    private void HandleSinglePlayer()
    {
        LoadScene("SinglePlayerGame");
    }

    // --------------------------------------------------------
    // HELPERS
    // --------------------------------------------------------

    /// <summary>
    /// Generic async scene loader that ensures proper unloading & initialisation.
    /// </summary>
    private void LoadScene(string sceneName)
    {
        Debug.Log($"[GameManager] Loading scene: {sceneName}");
        SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
    }
}

/// <summary>
/// Main game states.
/// </summary>
[Serializable]
public enum GameState
{
    None = -1,
    Starting = 0,
    MainMenu = 1,
    DeckMenu = 2,
    SettingsMenu = 3,
    SinglePlayer = 4,
}