using UnityEngine;

public class Pipe : MonoBehaviour {
    [SerializeField] private float speed = -3f;
    [SerializeField] private float despawnX = -12f;

    private void Update() {
        if (GameManager.Instance == null) return;
        if (GameManager.Instance.CurrentState == GameState.GameOver || GameManager.Instance.IsBirdDead || GameManager.Instance.CurrentState == GameState.Paused) return;

        // Move left
        transform.Translate(Vector3.right * speed * Time.deltaTime, Space.World);

        // Despawn if out of view
        if (transform.position.x < despawnX) {
            gameObject.SetActive(false);
        }
    }
}
