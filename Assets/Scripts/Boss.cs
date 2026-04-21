using System.Collections;
using UnityEngine;

public class Boss : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private string playerTag = "Player";

    [Header("Boss Movement")]
    private bool keepBossStaticInAir = true;

    [Header("Strike Timing")]
    [SerializeField] private float strikeInterval = 2f;
    [SerializeField] private float strikeDelay = 2f;

    [Header("Strike Settings")]
    [SerializeField] private int strikeDamage = 50;
    [SerializeField] private float strikeRadius = 2.5f;

    [Header("Ground Targeting")]
    private LayerMask groundMask = ~0;
    [SerializeField] private float raycastStartHeight = 30f;
    [SerializeField] private float raycastDistance = 120f;

    [Header("Visual Warning")]
    [SerializeField] private GameObject warningCirclePrefab;
    [SerializeField] private float warningYOffset = 0.03f;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip explosionSound;

    [Header("Health")]
    [SerializeField] private int maxHealth = 3000;
    [SerializeField] private Color hitBlinkColor = Color.white;
    private float hitBlinkDuration = 0.08f;
    private int currentHealth;
    private Material[] blinkMaterials;
    private Color[] defaultBlinkColors;
    private Coroutine hitBlinkRoutine;

    [Header("Victory")]
    [SerializeField] private GameObject victoryPanel;

    public static bool IsVictory { get; private set; }

    private float nextStrikeTime;
    private Vector3 lockedPosition;

    // Initialisiert Referenzen und Startwerte.
    private void Awake()
    {
        lockedPosition = transform.position;
        currentHealth = maxHealth;

        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        blinkMaterials = new Material[renderers.Length];
        defaultBlinkColors = new Color[renderers.Length];
        for (int i = 0; i < renderers.Length; i++)
        {
            blinkMaterials[i] = renderers[i].material;
            defaultBlinkColors[i] = blinkMaterials[i].color;
        }

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.useGravity = false;
            rb.isKinematic = true;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }

    // Aktualisiert die Logik in jedem Frame.
    private void Update()
    {
        EnsurePlayerReference();
        if (playerTransform == null)
        {
            return;
        }

        if (Time.time < nextStrikeTime)
        {
            return;
        }

        nextStrikeTime = Time.time + strikeInterval;
        StartCoroutine(StrikeRoutine());
    }

    // Enthält die Logik für LateUpdate.
    private void LateUpdate()
    {
        if (!keepBossStaticInAir)
        {
            return;
        }

        transform.position = lockedPosition;
    }

    // Stellt sicher, dass PlayerTransform vorhanden ist.
    private void EnsurePlayerReference()
    {
        if (playerTransform != null)
        {
            return;
        }

        GameObject playerObject = GameObject.FindGameObjectWithTag(playerTag);
        if (playerObject != null)
        {
            playerTransform = playerObject.transform;
        }
    }

    // Startet StrikeRoutine.
    private IEnumerator StrikeRoutine()
    {
        Vector3 targetAtCastTime = playerTransform.position;
        Vector3 strikePoint = FindGroundPoint(targetAtCastTime);

        GameObject warning = SpawnWarning(strikePoint);

        yield return new WaitForSeconds(strikeDelay);

        ApplyStrikeDamage(strikePoint);
        PlayExplosionSound(strikePoint);

        if (warning != null)
        {
            Destroy(warning);
        }
    }

    // Liefert GroundPoint zurück und ignoriert dabei den eigenen Boss-Collider.
    private Vector3 FindGroundPoint(Vector3 around)
    {
        Vector3 rayStart = around + Vector3.up * raycastStartHeight;
        int bossLayerMask = groundMask & ~(1 << gameObject.layer);
        if (Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, raycastDistance, bossLayerMask, QueryTriggerInteraction.Ignore))
        {
            return hit.point;
        }

        return around;
    }

    // Enthält die Logik für SpawnWarning.
    private GameObject SpawnWarning(Vector3 strikePoint)
    {
        if (warningCirclePrefab == null)
        {
            return null;
        }

        Vector3 spawnPos = strikePoint + Vector3.up * warningYOffset;
        GameObject warning = Instantiate(warningCirclePrefab, spawnPos, Quaternion.identity);

        Vector3 scale = warning.transform.localScale;
        float diameter = strikeRadius * 2f;
        warning.transform.localScale = new Vector3(diameter, scale.y, diameter);

        return warning;
    }

    // Wendet StrikeDamage an.
    private void ApplyStrikeDamage(Vector3 strikePoint)
    {
        if (playerTransform == null)
        {
            return;
        }

        Vector3 playerPos = playerTransform.position;
        playerPos.y = 0f;

        Vector3 point = strikePoint;
        point.y = 0f;

        float sqrDistance = (playerPos - point).sqrMagnitude;
        if (sqrDistance > strikeRadius * strikeRadius)
        {
            return;
        }

        PlayerHealth health = playerTransform.GetComponent<PlayerHealth>();
        if (health == null)
        {
            health = playerTransform.GetComponentInParent<PlayerHealth>();
        }

        if (health != null)
        {
            health.TakeDamage(strikeDamage);
        }
    }

    // Spielt Explosions-Sound beim Einschlag ab.
    private void PlayExplosionSound(Vector3 location)
    {
        if (audioSource == null || explosionSound == null)
        {
            return;
        }

        audioSource.transform.position = location;
        audioSource.PlayOneShot(explosionSound);
    }

    // Mittelpunkt des Bosses als Zielpunkt für die Trefferberechnung.
    public Vector3 AimPoint => transform.position;

    // Trefferradius des Bosses für die Schusserkennung.
    public float HitRadius => 1.5f;

    // Verarbeitet eingehenden Schaden und zerstört den Boss bei 0 HP.
    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        TriggerHitBlink();
        if (currentHealth <= 0)
        {
            ShowVictory();
            Destroy(gameObject);
        }
    }

    // Zeigt Victory-Panel an, pausiert das Spiel und gibt Cursor frei.
    private void ShowVictory()
    {
        IsVictory = true;

        if (victoryPanel != null)
        {
            victoryPanel.SetActive(true);
        }

        Time.timeScale = 0f;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    // Löst Hit-Blink-Effekt aus.
    private void TriggerHitBlink()
    {
        if (blinkMaterials == null || blinkMaterials.Length == 0)
        {
            return;
        }

        if (hitBlinkRoutine != null)
        {
            StopCoroutine(hitBlinkRoutine);
        }

        hitBlinkRoutine = StartCoroutine(HitBlinkRoutine());
    }

    // Blinkt kurz in hitBlinkColor
    private IEnumerator HitBlinkRoutine()
    {
        for (int i = 0; i < blinkMaterials.Length; i++)
        {
            if (blinkMaterials[i] != null)
            {
                blinkMaterials[i].color = hitBlinkColor;
            }
        }

        yield return new WaitForSeconds(hitBlinkDuration);

        for (int i = 0; i < blinkMaterials.Length; i++)
        {
            if (blinkMaterials[i] != null)
            {
                blinkMaterials[i].color = defaultBlinkColors[i];
            }
        }

        hitBlinkRoutine = null;
    }
}
