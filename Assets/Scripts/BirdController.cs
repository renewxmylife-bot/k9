using UnityEngine;

public class BirdController : MonoBehaviour {
    [Header("Movement")]
    [SerializeField] private float flapForce = 5f;
    [SerializeField] private float maxFallSpeed = -10f;
    [SerializeField] private float gravityScale = 1.8f;
    
    [Header("Sprites")]
    [SerializeField] private Sprite normalSprite;
    [SerializeField] private Sprite flapSprite;
    [SerializeField] private Sprite dizzySprite;
    [SerializeField] private Sprite dieSprite;
    
    [Header("References")]
    [SerializeField] private ParticleSystem deathParticleSystem;

    [Header("Invincibility Settings")]
    [SerializeField] private float invincibilityDuration = 2f;

    [Header("Aura Settings")]
    [SerializeField] private SpriteRenderer auraSpriteRenderer;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Collider2D birdCollider;
    private bool isDead = false;
    private Vector3 startPosition;

    private int currentLives = 5;
    private bool isInvincible = false;

    public static event System.Action<int> OnLivesChanged;

    public static BirdController Instance { get; private set; }

    private void Awake() {
        Instance = this;
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        birdCollider = GetComponent<Collider2D>();
        startPosition = transform.position;
        rb.bodyType = RigidbodyType2D.Kinematic; // Set kinematic until game starts
        rb.constraints = RigidbodyConstraints2D.FreezeRotation | RigidbodyConstraints2D.FreezePositionX; // Stop physics from rotating or pushing the bird horizontally
        rb.gravityScale = gravityScale; // Make gravity feel snappy
    }

    private void OnEnable() {
        GameManager.OnGameStarted += StartPhysics;
        GameManager.OnGameOver += HandleDeath;
    }

    private void OnDisable() {
        GameManager.OnGameStarted -= StartPhysics;
        GameManager.OnGameOver -= HandleDeath;
    }

    private void StartPhysics() {
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.linearVelocity = Vector2.zero; // Reset velocity
        isDead = false;

        if (TelegramManager.Instance != null) {
            currentLives = TelegramManager.Instance.syncedLives;
        } else {
            currentLives = 5;
        }

        if (TelegramManager.Instance != null) {
            UpdateAuraVisual(TelegramManager.Instance.activeSkinName);
        }

        isInvincible = false;
        if (birdCollider != null) {
            birdCollider.isTrigger = false;
        }
        OnLivesChanged?.Invoke(currentLives);
        Flap();
    }

    private void HandleDeath() {
        isDead = true;
        if (spriteRenderer != null && normalSprite != null) {
            spriteRenderer.sprite = normalSprite;
        }
        
        // Disable colliders on all active pipes so the bird falls through them to the ground
        var pipePairs = GameObject.FindObjectsByType<Pipe>();
        foreach (var pair in pipePairs) {
            var colliders = pair.GetComponentsInChildren<Collider2D>();
            foreach (var col in colliders) {
                col.enabled = false;
            }
        }
        
        // Play death particles if available
        if (deathParticleSystem != null) {
            deathParticleSystem.Play();
        }

        if (AudioManager.Instance != null) {
            AudioManager.Instance.PlayHit();
            AudioManager.Instance.PlayDie();
        }
    }

    private void Update() {
        if (GameManager.Instance == null) return;

        if (GameManager.Instance.CurrentState == GameState.MainMenu) {
            // Slight floating animation
            float newY = startPosition.y + Mathf.Sin(Time.time * 6f) * 0.15f;
            transform.position = new Vector3(startPosition.x, newY, startPosition.z);
            transform.rotation = Quaternion.identity;
            if (spriteRenderer != null && normalSprite != null) {
                spriteRenderer.sprite = normalSprite;
            }
            return;
        }

        if (GameManager.Instance.CurrentState != GameState.Playing || isDead) {
            // Apply falling rotation even during death fall
            if (isDead) {
                ApplyRotation();
            }
            return;
        }

        // Lock X position to prevent drifting or being pushed horizontally by pipes/physics
        transform.position = new Vector3(startPosition.x, transform.position.y, transform.position.z);

        // Update sprite based on vertical velocity (if not in invincibility/stun state)
        if (spriteRenderer != null && rb != null && !isInvincible) {
            if (rb.linearVelocity.y > 0.1f) {
                if (flapSprite != null) spriteRenderer.sprite = flapSprite;
            } else {
                if (normalSprite != null) spriteRenderer.sprite = normalSprite;
            }
        }

        // Check for inputs (responsive to Space, mouse clicks, and touches)
        bool jumpPressed = false;
#if ENABLE_INPUT_SYSTEM
        // Keyboard space
        if (UnityEngine.InputSystem.Keyboard.current != null && UnityEngine.InputSystem.Keyboard.current.spaceKey.wasPressedThisFrame) {
            jumpPressed = true;
        }
        // Mouse Left Click
        if (UnityEngine.InputSystem.Mouse.current != null && UnityEngine.InputSystem.Mouse.current.leftButton.wasPressedThisFrame) {
            bool isOverUI = UnityEngine.EventSystems.EventSystem.current != null && UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject();
            UnityEngine.Debug.Log($"[INPUT] Mouse clicked! IsOverUI: {isOverUI}");
            if (!isOverUI) {
                jumpPressed = true;
            }
        }
        // Touchscreen Tap
        if (UnityEngine.InputSystem.Touchscreen.current != null && UnityEngine.InputSystem.Touchscreen.current.primaryTouch.press.wasPressedThisFrame) {
            int touchId = UnityEngine.InputSystem.Touchscreen.current.primaryTouch.touchId.ReadValue();
            bool isOverUI = UnityEngine.EventSystems.EventSystem.current != null && UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject(touchId);
            UnityEngine.Debug.Log($"[INPUT] Touch tapped! TouchId: {touchId}, IsOverUI: {isOverUI}");
            if (!isOverUI) {
                jumpPressed = true;
            }
        }
#else
        if (Input.GetKeyDown(KeyCode.Space) || (Input.GetMouseButtonDown(0) && (UnityEngine.EventSystems.EventSystem.current == null || !UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject()))) {
            jumpPressed = true;
        }
#endif

        if (jumpPressed) {
            Flap();
        }

        ApplyRotation();
    }

    private void FixedUpdate() {
        if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameState.Playing || isDead) return;

        // Cap fall speed to avoid building too much velocity
        if (rb.linearVelocity.y < maxFallSpeed) {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, maxFallSpeed);
        }
    }

    private void Flap() {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, flapForce);
        UnityEngine.Debug.Log($"[FLAP] Flap called! rb.linearVelocity set to {rb.linearVelocity}");
        if (AudioManager.Instance != null) {
            AudioManager.Instance.PlayFlap();
        }
    }

    public void FlapFromUI() {
        if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameState.Playing || isDead) return;
        Flap();
    }

    private void ApplyRotation() {
        // Tilt bird based on vertical velocity
        float targetAngle = 0f;
        if (rb.linearVelocity.y > 0) {
            targetAngle = Mathf.Lerp(0f, 25f, rb.linearVelocity.y / flapForce);
        } else {
            targetAngle = Mathf.Lerp(0f, -90f, -rb.linearVelocity.y / 10f);
        }
        transform.rotation = Quaternion.Euler(0, 0, targetAngle);
    }

    private void OnCollisionEnter2D(Collision2D collision) {
        if (isDead || isInvincible) {
            // Keep original ground logic if already dead
            if (isDead && collision.gameObject.name.Contains("Ground")) {
                if (GameManager.Instance != null) {
                    GameManager.Instance.EndGame();
                }
            }
            return;
        }

        bool isGround = collision.gameObject.name.Contains("Ground");
        HandleDamage(isGround);
    }

    private void OnTriggerEnter2D(Collider2D other) {
        if (isDead) return;

        // While invincible/stunned, we might overlap with ground since isTrigger = true.
        // We still want to handle ground collisions as damage.
        if (other.gameObject.name.Contains("Ground")) {
            HandleDamage(true);
        }
    }

    private void HandleDamage(bool isGround) {
        if (currentLives > 0) {
            currentLives--;
            OnLivesChanged?.Invoke(currentLives);

            // Sync subtraction to backend database
            if (TelegramManager.Instance != null) {
                TelegramManager.Instance.SubtractLifeInBackend(1);
            }

            if (currentLives > 0) {
                if (AudioManager.Instance != null) {
                    AudioManager.Instance.PlayHit();
                }
                StartCoroutine(InvincibilityRoutine(isGround));
            } else {
                // No lives left - die for real
                if (GameManager.Instance != null) {
                    GameManager.Instance.BirdDied();
                    if (isGround) {
                        GameManager.Instance.EndGame();
                    }
                }
                HandleDeath();
            }
        }
    }

    private System.Collections.IEnumerator InvincibilityRoutine(bool hitGround) {
        isInvincible = true;
        if (birdCollider != null) {
            birdCollider.isTrigger = true;
        }

        if (hitGround) {
            // Teleport to center (starting position)
            transform.position = startPosition;
            if (rb != null) {
                rb.linearVelocity = Vector2.zero;
            }
            // Show dieSprite
            if (spriteRenderer != null && dieSprite != null) {
                spriteRenderer.sprite = dieSprite;
            }
        } else {
            // Show dizzySprite
            if (spriteRenderer != null && dizzySprite != null) {
                spriteRenderer.sprite = dizzySprite;
            }
        }

        // Flashing visual feedback
        float elapsed = 0f;
        float flashInterval = 0.1f;
        while (elapsed < invincibilityDuration) {
            if (spriteRenderer != null) {
                spriteRenderer.enabled = !spriteRenderer.enabled;
            }
            yield return new WaitForSeconds(flashInterval);
            elapsed += flashInterval;
        }

        // Restore state
        if (spriteRenderer != null) {
            spriteRenderer.enabled = true;
            if (normalSprite != null) {
                spriteRenderer.sprite = normalSprite;
            }
        }
        if (birdCollider != null) {
            birdCollider.isTrigger = false;
        }
        isInvincible = false;
    }

    public void SetLives(int lives) {
        currentLives = lives;
        OnLivesChanged?.Invoke(currentLives);
    }

    public void UpdateAuraVisual(string skinName) {
        if (auraSpriteRenderer == null) {
            Transform auraTransform = transform.Find("Aura");
            if (auraTransform != null) {
                auraSpriteRenderer = auraTransform.GetComponent<SpriteRenderer>();
            }
        }

        if (auraSpriteRenderer == null) return;

        if (string.IsNullOrEmpty(skinName) || skinName == "default") {
            auraSpriteRenderer.gameObject.SetActive(false);
            return;
        }

        auraSpriteRenderer.gameObject.SetActive(true);
        Color auraColor = Color.white;
        switch (skinName.ToLower()) {
            case "red":
                auraColor = Color.red;
                break;
            case "yellow":
                auraColor = Color.yellow;
                break;
            case "green":
                auraColor = Color.green;
                break;
            case "blue":
                auraColor = new Color(0f, 0.5f, 1f, 1f); // bright blue
                break;
            case "orange":
                auraColor = new Color(1f, 0.5f, 0f, 1f); // orange
                break;
            default:
                auraSpriteRenderer.gameObject.SetActive(false);
                return;
        }

        // Set alpha to make it a transparent glowing effect
        auraColor.a = 0.5f;
        auraSpriteRenderer.color = auraColor;
    }
}
