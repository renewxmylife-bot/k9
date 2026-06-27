using UnityEngine;

public class PipeSpawner : MonoBehaviour {
    [Header("Spawn Settings")]
    [SerializeField] private float spawnRate = 2f;
    [SerializeField] private float minYOffset = -2.5f;
    [SerializeField] private float maxYOffset = 2.5f;
    
    private float spawnTimer = 0f;

    private void Start() {
        if (GameManager.Instance != null && GameManager.Instance.CurrentState == GameState.Playing) {
            SpawnPipe();
        }
    }

    private void OnEnable() {
        GameManager.OnGameStarted += ResetSpawner;
    }

    private void OnDisable() {
        GameManager.OnGameStarted -= ResetSpawner;
    }

    private void ResetSpawner() {
        spawnTimer = 0f;
        if (ObjectPool.Instance != null) {
            ObjectPool.Instance.ReturnAllToPool();
        }
        SpawnPipe();
    }

    private void Update() {
        if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameState.Playing || GameManager.Instance.IsBirdDead) return;

        spawnTimer += Time.deltaTime;
        if (spawnTimer >= spawnRate) {
            SpawnPipe();
            spawnTimer = 0f;
        }
    }

    private void SpawnPipe() {
        if (ObjectPool.Instance == null) return;

        GameObject pipe = ObjectPool.Instance.GetPooledObject();
        if (pipe != null) {
            float yOffset = Random.Range(minYOffset, maxYOffset);
            pipe.transform.position = new Vector3(transform.position.x, yOffset, 0f);
            pipe.SetActive(true);
        }
    }
}
