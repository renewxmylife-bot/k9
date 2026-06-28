using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour {
    public static UIManager Instance { get; private set; }

    [Header("UI Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject hudPanel;
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject gameOverPanel;

    [Header("Texts")]
    [SerializeField] private TMP_Text currentScoreText;
    [SerializeField] private TMP_Text finalScoreText;
    [SerializeField] private TMP_Text bestScoreText;
    [SerializeField] private TMP_Text livesText;

    [Header("Buttons")]
    [SerializeField] private Button playButton;
    [SerializeField] private Button pauseButton;
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button jumpButton;
    [SerializeField] private Button mainMenuButton;

    private void Awake() {
        if (Instance == null) {
            Instance = this;
        } else {
            Destroy(gameObject);
            return;
        }

        // Auto-detect lives text if not assigned
        if (livesText == null) {
            var liveImg = GameObject.Find("Canvas/HUDPanel/LiveImg");
            if (liveImg != null) {
                livesText = liveImg.transform.Find("Text")?.GetComponent<TMP_Text>();
            }
        }

        // Auto-detect mainMenuButton if not assigned
        if (mainMenuButton == null) {
            var mBtnObj = GameObject.Find("Canvas/SafeAreaPanel/GameOverPanel/MainMenu");
            if (mBtnObj != null) {
                mainMenuButton = mBtnObj.GetComponent<Button>();
            }
        }

        // Add button listeners
        if (playButton != null) playButton.onClick.AddListener(OnPlayPressed);
        if (pauseButton != null) pauseButton.onClick.AddListener(OnPausePressed);
        if (resumeButton != null) resumeButton.onClick.AddListener(OnResumePressed);
        if (restartButton != null) restartButton.onClick.AddListener(OnRestartPressed);
        if (jumpButton != null) jumpButton.onClick.AddListener(OnJumpPressed);
        if (mainMenuButton != null) mainMenuButton.onClick.AddListener(OnMainMenuPressed);
    }

    private void Start() {
        if (GameManager.Instance != null && GameManager.Instance.CurrentState == GameState.MainMenu) {
            ShowMainMenu();
        }
        CreateHUDMainMenuButton();
    }

    private void OnEnable() {
        GameManager.OnGameStarted += ShowHUD;
        GameManager.OnGameOver += ShowGameOver;
        GameManager.OnGamePaused += ShowPauseScreen;
        GameManager.OnGameResumed += HidePauseScreen;
        
        ScoreManager.OnScoreChanged += UpdateScoreText;
        BirdController.OnLivesChanged += UpdateLivesText;
    }

    private void OnDisable() {
        GameManager.OnGameStarted -= ShowHUD;
        GameManager.OnGameOver -= ShowGameOver;
        GameManager.OnGamePaused -= ShowPauseScreen;
        GameManager.OnGameResumed -= HidePauseScreen;

        ScoreManager.OnScoreChanged -= UpdateScoreText;
        BirdController.OnLivesChanged -= UpdateLivesText;
    }

    private void ShowMainMenu() {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
        if (hudPanel != null) hudPanel.SetActive(false);
        if (pausePanel != null) pausePanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
    }

    private void ShowHUD() {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (hudPanel != null) hudPanel.SetActive(true);
        if (pausePanel != null) pausePanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        
        UpdateScoreText(0);
    }

    public void UpdateLivesText(int lives) {
        if (livesText != null) {
            livesText.text = lives.ToString();
        }
    }

    private void ShowPauseScreen() {
        if (pausePanel != null) pausePanel.SetActive(true);
    }

    private void HidePauseScreen() {
        if (pausePanel != null) pausePanel.SetActive(false);
    }

    private void ShowGameOver() {
        if (hudPanel != null) hudPanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(true);

        if (ScoreManager.Instance != null) {
            if (finalScoreText != null) finalScoreText.text = "Score: " + ScoreManager.Instance.Score;
            if (bestScoreText != null) bestScoreText.text = "Best: " + ScoreManager.Instance.BestScore;
        }
    }

    private void UpdateScoreText(int score) {
        if (currentScoreText != null) {
            currentScoreText.text = score.ToString();
        }
    }

    private void OnPlayPressed() {
        if (GameManager.Instance != null) {
            GameManager.Instance.StartGame();
        }
    }

    private void OnPausePressed() {
        if (GameManager.Instance != null) {
            GameManager.Instance.PauseGame();
        }
    }

    private void OnResumePressed() {
        if (GameManager.Instance != null) {
            GameManager.Instance.ResumeGame();
        }
    }

    private void OnRestartPressed() {
        if (TelegramManager.Instance != null && TelegramManager.Instance.syncedLives <= 0) {
            UnityEngine.Debug.Log("[UI] Player has 0 lives. Cannot restart. Opening lives shop...");
            TelegramManager.Instance.OpenTelegramShop("lives");
            return;
        }
        if (GameManager.Instance != null) {
            GameManager.Instance.RestartGame();
        }
    }

    private void OnJumpPressed() {
        if (BirdController.Instance != null) {
            BirdController.Instance.FlapFromUI();
        }
    }

    private void OnMainMenuPressed() {
        Time.timeScale = 1f;
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }

    private void CreateHUDMainMenuButton() {
        if (mainMenuButton != null && hudPanel != null) {
            // Instantiate a duplicate of the Main Menu button for the gameplay HUD
            Button hudMenuBtn = Instantiate(mainMenuButton, hudPanel.transform);
            hudMenuBtn.name = "HUDMainMenuButton";

            // Position it nicely on the right, directly below the Pause button
            RectTransform rect = hudMenuBtn.GetComponent<RectTransform>();
            if (rect != null) {
                rect.anchorMin = new Vector2(0.5f, 0.5f);
                rect.anchorMax = new Vector2(0.5f, 0.5f);
                rect.anchoredPosition = new Vector2(350f, 620f); // Directly below the Pause button
                rect.sizeDelta = new Vector2(250f, 100f); // Match the Pause button size
            }

            // Adjust text size so it fits inside the smaller button bounds
            var textComp = hudMenuBtn.GetComponentInChildren<TMP_Text>();
            if (textComp != null) {
                textComp.fontSize = 28f;
            }

            // Set up click action
            hudMenuBtn.onClick.RemoveAllListeners();
            hudMenuBtn.onClick.AddListener(OnMainMenuPressed);
        }
    }
}
