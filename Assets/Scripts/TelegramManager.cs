using System;
using System.Collections;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Networking;

public class TelegramManager : MonoBehaviour {
    public static TelegramManager Instance { get; private set; }

    [Header("Backend Configuration")]
    [SerializeField] private string backendUrl = "https://rknight.hoangsabelongtovn.site"; // Can be edited in inspector or runtime

    [Header("Player State Sync")]
    public long telegramId = 0;
    public string username = "EditorPlayer";
    public int syncedLives = 5;
    public bool isPremium = false;
    public string activeSkinName = "default";

    // JS import methods
#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern string GetTelegramInitData();

    [DllImport("__Internal")]
    private static extern void ShowShopOverlay(string shopType, string backendUrl, string telegramId, string username);

    [DllImport("__Internal")]
    private static extern void ConsoleLogAd(string msg);
#else
    private static string GetTelegramInitData() { return null; }
    private static void ShowShopOverlay(string shopType, string backendUrl, string telegramId, string username) { Debug.Log("[EDITOR] ShowShopOverlay: " + shopType + " (Backend: " + backendUrl + ", User: " + username + " - " + telegramId + ")"); }
    private static void ConsoleLogAd(string msg) { Debug.Log("[EDITOR] ConsoleLogAd: " + msg); }
#endif

    private void Awake() {
        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        } else {
            Destroy(gameObject);
            return;
        }

        Initialize();
    }

    private void Initialize() {
        string initData = GetTelegramInitData();

        if (!string.IsNullOrEmpty(initData)) {
            try {
                TelegramUser user = JsonUtility.FromJson<TelegramUser>(initData);
                telegramId = user.id;
                username = user.username;
                Debug.Log($"[TELEGRAM] Authenticated user: {username} ({telegramId})");
            } catch (Exception ex) {
                Debug.LogError("[TELEGRAM] Failed to parse init data: " + ex.Message);
            }
        } else {
            // Editor fallback or standalone web build
            telegramId = 123456789; // Mock ID
            username = "EditorPlayer";
            Debug.Log($"[TELEGRAM] Running outside of Telegram. Using mock user: {username} ({telegramId})");
        }

        // Fetch initial profile
        StartCoroutine(FetchUserProfile());
    }

    public void OpenTelegramShop(string shopType) {
        ShowShopOverlay(shopType, backendUrl, telegramId.ToString(), username);
    }

    public void TriggerAdvertisement(string msg) {
        if (!isPremium) {
            ConsoleLogAd(msg);
        } else {
            Debug.Log("[AD] Ad skipped (Premium user)");
        }
    }

    public IEnumerator FetchUserProfile() {
        string url = $"{backendUrl}/api/user/{telegramId}?username={UnityWebRequest.EscapeURL(username)}";
        using (UnityWebRequest webRequest = UnityWebRequest.Get(url)) {
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.Success) {
                string jsonResult = webRequest.downloadHandler.text;
                Debug.Log("[TELEGRAM] Profile fetched: " + jsonResult);
                OnUserSync(jsonResult);
            } else {
                Debug.LogError("[TELEGRAM] Failed to fetch user profile: " + webRequest.error);
            }
        }
    }

    public void OnUserSync(string jsonPayload) {
        try {
            BackendUser backendUser = JsonUtility.FromJson<BackendUser>(jsonPayload);
            syncedLives = backendUser.lives;
            isPremium = backendUser.is_premium != 0;
            activeSkinName = backendUser.active_skin;

            Debug.Log($"[TELEGRAM] User state synced. Lives={syncedLives}, Premium={isPremium}, ActiveSkin={activeSkinName}");

            // Update local game managers and controllers
            if (BirdController.Instance != null) {
                BirdController.Instance.SetLives(syncedLives);
                BirdController.Instance.UpdateAuraVisual(activeSkinName);
            }
        } catch (Exception ex) {
            Debug.LogError("[TELEGRAM] Failed to sync user state from payload: " + ex.Message);
        }
    }

    // Callbacks from JavaScript
    public void OnPurchaseSuccess(string updatedUserJson) {
        Debug.Log("[TELEGRAM] Purchase successful callback received.");
        if (!string.IsNullOrEmpty(updatedUserJson)) {
            OnUserSync(updatedUserJson);
        } else {
            StartCoroutine(FetchUserProfile());
        }
    }

    public void SubtractLifeInBackend(int amount) {
        StartCoroutine(SubtractLifeCoroutine(amount));
    }

    private IEnumerator SubtractLifeCoroutine(int amount) {
        string url = $"{backendUrl}/api/user/{telegramId}/subtract-lives";
        string jsonPayload = $"{{\"amount\":{amount}}}";
        byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(jsonPayload);

        using (UnityWebRequest webRequest = new UnityWebRequest(url, "POST")) {
            webRequest.uploadHandler = new UploadHandlerRaw(jsonToSend);
            webRequest.downloadHandler = new DownloadHandlerBuffer();
            webRequest.SetRequestHeader("Content-Type", "application/json");

            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.Success) {
                string jsonResult = webRequest.downloadHandler.text;
                Debug.Log("[TELEGRAM] Lives subtracted successfully on backend: " + jsonResult);
                OnUserSync(jsonResult);
            } else {
                Debug.LogError("[TELEGRAM] Failed to subtract life on backend: " + webRequest.error);
            }
        }
    }

    [Serializable]
    private class TelegramUser {
        public long id;
        public string username;
    }

    [Serializable]
    private class BackendUser {
        public long telegram_id;
        public string username;
        public int lives;
        public string active_skin;
        public int is_premium;
    }
}
