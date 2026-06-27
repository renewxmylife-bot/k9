using UnityEngine;
using System;

public class ScoreManager : MonoBehaviour {
    public static ScoreManager Instance { get; private set; }

    public int Score { get; private set; } = 0;
    public int BestScore { get; private set; } = 0;

    public static event Action<int> OnScoreChanged;
    public static event Action<int> OnBestScoreChanged;

    private const string BestScoreKey = "FlappyBirdBestScore";

    private void Awake() {
        if (Instance == null) {
            Instance = this;
        } else {
            Destroy(gameObject);
            return;
        }

        BestScore = PlayerPrefs.GetInt(BestScoreKey, 0);
    }

    public void AddScore(int amount = 1) {
        if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameState.Playing) return;

        Score += amount;
        OnScoreChanged?.Invoke(Score);

        if (Score > BestScore) {
            BestScore = Score;
            PlayerPrefs.SetInt(BestScoreKey, BestScore);
            PlayerPrefs.Save();
            OnBestScoreChanged?.Invoke(BestScore);
        }
    }

    public void ResetScore() {
        Score = 0;
        OnScoreChanged?.Invoke(Score);
    }
}
