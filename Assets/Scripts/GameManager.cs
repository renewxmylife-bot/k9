using UnityEngine;
using System;

public enum GameState {
    MainMenu,
    Playing,
    Paused,
    GameOver
}

public class GameManager : MonoBehaviour {
    public static GameManager Instance { get; private set; }

    public GameState CurrentState { get; private set; } = GameState.MainMenu;
    public bool IsBirdDead { get; private set; } = false;

    public static event Action OnGameStarted;
    public static event Action OnBirdDied;
    public static event Action OnGameOver;
    public static event Action OnGamePaused;
    public static event Action OnGameResumed;

    private void Awake() {
        if (Instance == null) {
            Instance = this;
        } else {
            Destroy(gameObject);
            return;
        }
        
        Time.timeScale = 1f;
    }

    private void Start() {
        StartGame();
    }

    public void StartGame() {
        CurrentState = GameState.Playing;
        IsBirdDead = false;
        Time.timeScale = 1f;

        if (TelegramManager.Instance != null) {
            TelegramManager.Instance.TriggerAdvertisement("Game Started");
        }

        OnGameStarted?.Invoke();
    }

    public void PauseGame() {
        if (CurrentState == GameState.Playing) {
            CurrentState = GameState.Paused;
            Time.timeScale = 0f;
            OnGamePaused?.Invoke();
        }
    }

    public void ResumeGame() {
        if (CurrentState == GameState.Paused) {
            CurrentState = GameState.Playing;
            Time.timeScale = 1f;
            OnGameResumed?.Invoke();
        }
    }

    public void BirdDied() {
        if (CurrentState == GameState.Playing) {
            IsBirdDead = true;
            OnBirdDied?.Invoke();
        }
    }

    public void EndGame() {
        // Can be called directly or after bird hits ground
        CurrentState = GameState.GameOver;
        Time.timeScale = 0f;
        OnGameOver?.Invoke();
    }

    public void RestartGame() {
        Time.timeScale = 1f;
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
    }
}
