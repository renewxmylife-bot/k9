using UnityEngine;

public class ScrollingObject : MonoBehaviour {
    [SerializeField] private float scrollSpeed = -3f;
    
    private float spriteWidth;
    private Vector3 startPosition;

    private void Awake() {
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null) {
            // Try to look in children
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        if (spriteRenderer != null) {
            spriteWidth = spriteRenderer.bounds.size.x;
        } else {
            spriteWidth = 20f;
        }
        startPosition = transform.position;
    }

    private void Update() {
        if (GameManager.Instance == null) return;
        
        // Stop scrolling when the game is over
        if (GameManager.Instance.CurrentState == GameState.GameOver) return;
        if (GameManager.Instance.IsBirdDead) return;
        if (GameManager.Instance.CurrentState == GameState.Paused) return;

        // Move to the left
        transform.Translate(Vector3.right * scrollSpeed * Time.deltaTime, Space.World);

        // Reset position when moved by the width
        if (transform.position.x <= startPosition.x - spriteWidth) {
            transform.position += Vector3.right * spriteWidth;
        }
    }
}
