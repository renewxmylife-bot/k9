using UnityEngine;

public class ScoreTrigger : MonoBehaviour {
    private void OnTriggerExit2D(Collider2D other) {
        if (other.CompareTag("Player") || other.GetComponent<BirdController>() != null) {
            if (ScoreManager.Instance != null) {
                ScoreManager.Instance.AddScore(1);
            }
            if (AudioManager.Instance != null) {
                AudioManager.Instance.PlayPoint();
            }
        }
    }
}
