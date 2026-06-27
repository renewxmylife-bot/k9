using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour {
    public static ObjectPool Instance { get; private set; }

    [SerializeField] private GameObject prefab;
    [SerializeField] private int poolSize = 5;
    [SerializeField] private bool shouldGrow = true;

    private List<GameObject> pooledObjects;

    private void Awake() {
        if (Instance == null) {
            Instance = this;
        } else {
            Destroy(gameObject);
            return;
        }

        InitializePool();
    }

    private void InitializePool() {
        pooledObjects = new List<GameObject>();
        for (int i = 0; i < poolSize; i++) {
            CreateNewObject();
        }
    }

    private GameObject CreateNewObject() {
        GameObject obj = Instantiate(prefab, transform);
        obj.SetActive(false);
        pooledObjects.Add(obj);
        return obj;
    }

    public GameObject GetPooledObject() {
        for (int i = 0; i < pooledObjects.Count; i++) {
            if (pooledObjects[i] != null && !pooledObjects[i].activeInHierarchy) {
                return pooledObjects[i];
            }
        }

        if (shouldGrow) {
            return CreateNewObject();
        }

        return null;
    }

    public void ReturnAllToPool() {
        if (pooledObjects == null) return;
        for (int i = 0; i < pooledObjects.Count; i++) {
            if (pooledObjects[i] != null) {
                pooledObjects[i].SetActive(false);
            }
        }
    }
}
