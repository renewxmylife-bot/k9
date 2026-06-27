using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour {
    [SerializeField] private Button playButton;
    [SerializeField] private Button buyLivesButton;
    [SerializeField] private Button buySkinsButton;
    [SerializeField] private Button premiumButton;

    private void Awake() {
        // Auto-detect and configure Play Button
        if (playButton == null) {
            var playBtnObj = GameObject.Find("Canvas/Panel/Play");
            if (playBtnObj != null) {
                playButton = playBtnObj.GetComponent<Button>();
                if (playButton == null) playButton = playBtnObj.AddComponent<Button>();
            }
        }
        if (playButton != null) {
            playButton.onClick.AddListener(OnPlayPressed);
            UnityEngine.Debug.Log("[MAIN_MENU] Play button listener successfully added.");
        }

        // Auto-detect and configure Buy Lives Button
        if (buyLivesButton == null) {
            var buyLivesObj = GameObject.Find("Canvas/Panel/Buy Lives");
            if (buyLivesObj != null) {
                buyLivesButton = buyLivesObj.GetComponent<Button>();
                if (buyLivesButton == null) buyLivesButton = buyLivesObj.AddComponent<Button>();
            }
        }
        if (buyLivesButton != null) {
            buyLivesButton.onClick.AddListener(OnBuyLivesPressed);
            UnityEngine.Debug.Log("[MAIN_MENU] Buy Lives button listener successfully added.");
        }

        // Auto-detect and configure Buy Skins Button
        if (buySkinsButton == null) {
            var buySkinsObj = GameObject.Find("Canvas/Panel/Buy Skin");
            if (buySkinsObj != null) {
                buySkinsButton = buySkinsObj.GetComponent<Button>();
                if (buySkinsButton == null) buySkinsButton = buySkinsObj.AddComponent<Button>();
            }
        }
        if (buySkinsButton != null) {
            buySkinsButton.onClick.AddListener(OnBuySkinsPressed);
            UnityEngine.Debug.Log("[MAIN_MENU] Buy Skins button listener successfully added.");
        }

        // Auto-detect and configure Premium Button
        if (premiumButton == null) {
            var premiumObj = GameObject.Find("Canvas/Panel/Premium");
            if (premiumObj != null) {
                premiumButton = premiumObj.GetComponent<Button>();
                if (premiumButton == null) premiumButton = premiumObj.AddComponent<Button>();
            }
        }
        if (premiumButton != null) {
            premiumButton.onClick.AddListener(OnPremiumPressed);
            UnityEngine.Debug.Log("[MAIN_MENU] Premium button listener successfully added.");
        }
    }

    public void OnPlayPressed() {
        Time.timeScale = 1f;
        UnityEngine.Debug.Log("[MAIN_MENU] Play button pressed. Loading FlappyBirdScene...");
        SceneManager.LoadScene("FlappyBirdScene");
    }

    public void OnBuyLivesPressed() {
        UnityEngine.Debug.Log("[MAIN_MENU] Buy Lives button pressed. Requesting shop overlay...");
        if (TelegramManager.Instance != null) {
            TelegramManager.Instance.OpenTelegramShop("lives");
        } else {
            UnityEngine.Debug.LogWarning("[MAIN_MENU] TelegramManager Instance not found!");
        }
    }

    public void OnBuySkinsPressed() {
        UnityEngine.Debug.Log("[MAIN_MENU] Buy Skins button pressed. Requesting shop overlay...");
        if (TelegramManager.Instance != null) {
            TelegramManager.Instance.OpenTelegramShop("skins");
        } else {
            UnityEngine.Debug.LogWarning("[MAIN_MENU] TelegramManager Instance not found!");
        }
    }

    public void OnPremiumPressed() {
        UnityEngine.Debug.Log("[MAIN_MENU] Premium button pressed. Requesting shop overlay...");
        if (TelegramManager.Instance != null) {
            TelegramManager.Instance.OpenTelegramShop("premium");
        } else {
            UnityEngine.Debug.LogWarning("[MAIN_MENU] TelegramManager Instance not found!");
        }
    }
}
