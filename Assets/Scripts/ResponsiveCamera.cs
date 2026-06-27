using UnityEngine;

[RequireComponent(typeof(Camera))]
public class ResponsiveCamera : MonoBehaviour {
    [SerializeField] private float targetWidth = 10.8f; // 1080 pixels at 100px/unit
    [SerializeField] private float targetHeight = 19.2f; // 1920 pixels at 100px/unit

    private Camera cam;

    private void Awake() {
        cam = GetComponent<Camera>();
        AdjustCamera();
    }

    private void Update() {
        AdjustCamera();
    }

    private void AdjustCamera() {
        if (cam == null) return;

        float aspect = (float)Screen.width / Screen.height;
        float targetAspect = targetWidth / targetHeight;

        if (aspect >= targetAspect) {
            cam.orthographicSize = targetHeight / 2f;
        } else {
            cam.orthographicSize = (targetWidth / aspect) / 2f;
        }
    }
}
