using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour {
    [SerializeField] private Button playButton;

    private void Awake() {
        // Auto-detect Play button if not assigned
        if (playButton == null) {
            var playBtnObj = GameObject.Find("Canvas/Panel/Play");
            if (playBtnObj != null) {
                playButton = playBtnObj.GetComponent<Button>();
                if (playButton == null) {
                    playButton = playBtnObj.AddComponent<Button>();
                    UnityEngine.Debug.Log("[MAIN_MENU] Added Button component to Play button GameObject.");
                }
            }
        }

        if (playButton != null) {
            playButton.onClick.AddListener(OnPlayPressed);
            UnityEngine.Debug.Log("[MAIN_MENU] Play button listener successfully added.");
        } else {
            UnityEngine.Debug.LogError("[MAIN_MENU] Play button not found!");
        }
    }

    public void OnPlayPressed() {
        Time.timeScale = 1f;
        UnityEngine.Debug.Log("[MAIN_MENU] Play button pressed. Loading FlappyBirdScene...");
        SceneManager.LoadScene("FlappyBirdScene");
    }
}
