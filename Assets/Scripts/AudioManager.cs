using UnityEngine;

public class AudioManager : MonoBehaviour {
    public static AudioManager Instance { get; private set; }

    [Header("Audio Clips")]
    public AudioClip flapSound;
    public AudioClip pointSound;
    public AudioClip hitSound;
    public AudioClip dieSound;

    private AudioSource audioSource;

    private void Awake() {
        if (Instance == null) {
            Instance = this;
        } else {
            Destroy(gameObject);
            return;
        }

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    public void PlayFlap() {
        PlaySound(flapSound);
    }

    public void PlayPoint() {
        PlaySound(pointSound);
    }

    public void PlayHit() {
        PlaySound(hitSound);
    }

    public void PlayDie() {
        PlaySound(dieSound);
    }

    private void PlaySound(AudioClip clip) {
        if (clip != null && audioSource != null) {
            audioSource.PlayOneShot(clip);
        }
    }
}
