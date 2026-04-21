using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyAI : MonoBehaviour
{
    private enum EnemyState
    {
        Searching,
        Chasing,
        Attacking,
        Repositioning
    }

    [Header("References")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private Transform projectileSpawn;
    [SerializeField] private Projectile projectilePrefab;

    [Header("Detection")]
    [SerializeField] private float detectionRange = 18f;
    [SerializeField] private float attackRange = 9f;
    private float repathInterval = 1f;

    [Header("Shooting")]
    private float projectileSpeed = 20f;
    private float timeBetweenShots = 1f;
    private int minBurstShots = 1;
    private int maxBurstShots = 2;

    [Header("Melee")]
    [SerializeField] private bool meleeOnly;
    private int meleeDamage = 20;
    private float timeBetweenMeleeHits = 0.8f;

    [Header("Reposition")]
    private float repositionMinDistance = 2.5f;
    private float repositionMaxDistance = 4.5f;
    private float repositionStopDistance = 1f;

    [Header("Search Movement")]
    private float searchRadius = 2f;
    private float searchPointTolerance = 0.8f;
    private float searchPauseMinTime = 0.8f;
    private float searchPauseMaxTime = 1.8f;

    [Header("Health")]
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private Color hitBlinkColor = Color.white;
    private float hitBlinkDuration = 0.08f;
    private int currentHealth;

    private NavMeshAgent agent;
    private Rigidbody enemyRigidbody;
    private EnemyState state;
    private float nextRepathTime;
    private Vector3 searchCenter;
    private bool burstRoutineRunning;
    private bool isSearchPaused;
    private float searchResumeTime;
    private float nextMeleeHitTime;
    private Material[] blinkMaterials;
    private Color[] defaultBlinkColors;
    private Coroutine hitBlinkRoutine;
    private Collider mainCollider;
    private PlayerHealth cachedPlayerHealth;

    public Vector3 AimPoint
    {
        get
        {
            if (mainCollider != null)
            {
                return mainCollider.bounds.center;
            }

            return transform.position + Vector3.up;
        }
    }

    public float HitRadius
    {
        get
        {
            if (mainCollider != null)
            {
                Vector3 extents = mainCollider.bounds.extents;
                return Mathf.Max(extents.x, extents.y, extents.z);
            }

            return 0.75f;
        }
    }

    // Initialisiert Referenzen, Startzustand, Health und Materialien fuer den Treffer-Blink.
    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        enemyRigidbody = GetComponent<Rigidbody>();
        mainCollider = GetComponent<Collider>();
        searchCenter = transform.position;
        state = EnemyState.Searching;
        currentHealth = maxHealth;

        if (enemyRigidbody != null)
        {
            enemyRigidbody.useGravity = false;
            enemyRigidbody.isKinematic = true;
            enemyRigidbody.linearVelocity = Vector3.zero;
            enemyRigidbody.angularVelocity = Vector3.zero;
        }

        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        blinkMaterials = new Material[renderers.Length];
        defaultBlinkColors = new Color[renderers.Length];

        for (int i = 0; i < renderers.Length; i++)
        {
            blinkMaterials[i] = renderers[i].material;
            defaultBlinkColors[i] = blinkMaterials[i].color;
        }
    }

    private void Update()
    {
        EnsurePlayerReference();

        switch (state)
        {
            case EnemyState.Searching:
                UpdateSearching();
                break;
            case EnemyState.Chasing:
                UpdateChasing();
                break;
            case EnemyState.Attacking:
                UpdateAttacking();
                break;
            case EnemyState.Repositioning:
                UpdateRepositioning();
                break;
        }
    }

    // Sucht den Spieler über Tag, falls keine direkte Referenz gesetzt wurde.
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

    // Patrol-/Suchlogik: wartet, wählt Zufallspunkte und wechselt bei Sichtkontakt in Chasing.
    private void UpdateSearching()
    {
        if (CanSeePlayerInDetectionRange())
        {
            state = EnemyState.Chasing;
            isSearchPaused = false;

            if (CanUseAgent())
            {
                agent.isStopped = false;
            }

            return;
        }

        if (!CanUseAgent())
        {
            return;
        }

        if (isSearchPaused)
        {
            if (Time.time < searchResumeTime)
            {
                return;
            }

            isSearchPaused = false;
            agent.isStopped = false;
            SetNextSearchDestination();
            return;
        }

        bool reachedPoint = !agent.pathPending && agent.remainingDistance <= searchPointTolerance;
        if (reachedPoint)
        {
            agent.isStopped = true;
            isSearchPaused = true;
            searchResumeTime = Time.time + Random.Range(searchPauseMinTime, searchPauseMaxTime);
            return;
        }

        if (!agent.hasPath)
        {
            SetNextSearchDestination();
        }
    }

    // Setzt ein neues zufälliges NavMesh-Ziel rund um den Search-Center.
    private void SetNextSearchDestination()
    {
        Vector3 randomPoint = searchCenter + Random.insideUnitSphere * searchRadius;
        if (TryGetNavMeshPoint(randomPoint, searchRadius, out Vector3 navPoint))
        {
            agent.SetDestination(navPoint);
        }
    }

    // Verfolgt den Spieler, solange er im Detection-Bereich ist, und wechselt bei Nähe in Attacking.
    private void UpdateChasing()
    {
        if (playerTransform == null)
        {
            state = EnemyState.Searching;
            return;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        if (distanceToPlayer > detectionRange)
        {
            state = EnemyState.Searching;
            return;
        }

        if (distanceToPlayer <= attackRange)
        {
            state = EnemyState.Attacking;
            return;
        }

        if (CanUseAgent() && Time.time >= nextRepathTime)
        {
            nextRepathTime = Time.time + repathInterval;
            agent.SetDestination(playerTransform.position);
        }
    }

    // Stoppt Bewegung für Kampf, richtet den Gegner zum Spieler aus und startet Nahkampf oder Schuss-Burst.
    private void UpdateAttacking()
    {
        if (playerTransform == null)
        {
            state = EnemyState.Searching;
            return;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        if (distanceToPlayer > attackRange)
        {
            state = EnemyState.Chasing;
            return;
        }

        if (CanUseAgent())
        {
            agent.ResetPath();
        }

        FaceTarget(playerTransform.position);

        if (meleeOnly)
        {
            TryMeleeAttack();
            return;
        }

        if (!burstRoutineRunning)
        {
            StartCoroutine(FireBurstRoutine());
        }
    }

    // Verursacht in festem Intervall Nahkampfschaden am Spieler.
    private void TryMeleeAttack()
    {
        if (Time.time < nextMeleeHitTime)
        {
            return;
        }

        PlayerHealth playerHealth = GetPlayerHealth();
        if (playerHealth == null)
        {
            return;
        }

        playerHealth.TakeDamage(meleeDamage);
        nextMeleeHitTime = Time.time + timeBetweenMeleeHits;
    }

    // Cached den PlayerHealth-Zugriff, um wiederholte GetComponent-Calls zu vermeiden.
    private PlayerHealth GetPlayerHealth()
    {
        if (cachedPlayerHealth != null)
        {
            return cachedPlayerHealth;
        }

        if (playerTransform == null)
        {
            return null;
        }

        cachedPlayerHealth = playerTransform.GetComponent<PlayerHealth>();
        if (cachedPlayerHealth == null)
        {
            cachedPlayerHealth = playerTransform.GetComponentInParent<PlayerHealth>();
        }

        return cachedPlayerHealth;
    }

    // Wartet bis das Reposition-Ziel erreicht ist und entscheidet dann zwischen Attacking oder Chasing.
    private void UpdateRepositioning()
    {
        if (playerTransform == null)
        {
            state = EnemyState.Searching;
            return;
        }

        if (!CanUseAgent())
        {
            state = EnemyState.Searching;
            return;
        }

        if (!agent.pathPending && agent.remainingDistance <= repositionStopDistance)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
            state = distanceToPlayer <= attackRange ? EnemyState.Attacking : EnemyState.Chasing;
        }
    }

    // Prüft nur die Distanz zum Spieler als einfache Sicht-/Erkennungsbedingung.
    private bool CanSeePlayerInDetectionRange()
    {
        if (playerTransform == null)
        {
            return false;
        }

        return Vector3.Distance(transform.position, playerTransform.position) <= detectionRange;
    }

    // Schießt eine kurze Burst-Serie, danach repositioniert sich der Gegner für Bewegung im Kampf.
    private IEnumerator FireBurstRoutine()
    {
        burstRoutineRunning = true;

        int shotsToFire = Random.Range(minBurstShots, maxBurstShots + 1);

        for (int i = 0; i < shotsToFire; i++)
        {
            if (state != EnemyState.Attacking || playerTransform == null)
            {
                break;
            }

            FireSingleShot();
            yield return new WaitForSeconds(timeBetweenShots);
        }

        burstRoutineRunning = false;

        if (state == EnemyState.Attacking)
        {
            StartReposition();
        }
    }

    // Erzeugt ein Projektil in Richtung Spieler und übergibt Geschwindigkeit/Freund-Tag.
    private void FireSingleShot()
    {
        if (projectilePrefab == null || playerTransform == null)
        {
            return;
        }

        Transform spawnPoint = projectileSpawn != null ? projectileSpawn : transform;
        Vector3 direction = (playerTransform.position - spawnPoint.position).normalized;

        Projectile projectile = Instantiate(projectilePrefab, spawnPoint.position, Quaternion.LookRotation(direction, Vector3.up));
        projectile.Launch(direction, projectileSpeed, gameObject.tag, transform.root);
    }

    // Wählt seitliches Ausweichziel auf dem NavMesh, um nach Burst nicht statisch stehenzubleiben.
    private void StartReposition()
    {
        if (playerTransform == null)
        {
            state = EnemyState.Searching;
            return;
        }

        state = EnemyState.Repositioning;

        if (!CanUseAgent())
        {
            state = EnemyState.Chasing;
            return;
        }

        Vector3 toPlayer = (playerTransform.position - transform.position).normalized;
        Vector3 sideStep = Vector3.Cross(Vector3.up, toPlayer).normalized;
        float sideSign = Random.value > 0.5f ? 1f : -1f;
        float distance = Random.Range(repositionMinDistance, repositionMaxDistance);

        Vector3 targetPoint = transform.position + (sideStep * sideSign * distance);
        if (TryGetNavMeshPoint(targetPoint, repositionMaxDistance, out Vector3 navPoint))
        {
            agent.SetDestination(navPoint);
        }
        else
        {
            state = EnemyState.Chasing;
        }
    }

    // Dreht den Gegner weich auf die Zielposition (nur um die Y-Achse).
    private void FaceTarget(Vector3 targetPosition)
    {
        Vector3 flatDirection = targetPosition - transform.position;
        flatDirection.y = 0f;

        if (flatDirection.sqrMagnitude < 0.0001f)
        {
            return;
        }

        Quaternion targetRotation = Quaternion.LookRotation(flatDirection);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 12f);
    }

    // Sucht den nächsten gültigen Punkt auf dem NavMesh für sichere Agent-Ziele.
    private bool TryGetNavMeshPoint(Vector3 position, float maxDistance, out Vector3 navPoint)
    {
        if (NavMesh.SamplePosition(position, out NavMeshHit hit, maxDistance, NavMesh.AllAreas))
        {
            navPoint = hit.position;
            return true;
        }

        navPoint = Vector3.zero;
        return false;
    }

    // Schützt NavMesh-Aufrufe, falls Agent deaktiviert ist oder nicht auf dem NavMesh steht.
    private bool CanUseAgent()
    {
        return agent != null && agent.isActiveAndEnabled && agent.isOnNavMesh;
    }

    // Reduziert Health, triggert Treffer-Feedback und zerstört den Gegner bei 0 HP.
    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        TriggerHitBlink();
        if (currentHealth <= 0)
        {
            Destroy(gameObject);
        }
    }

    // Startet den kurzen Hit-Blink und ersetzt ggf. einen bereits laufenden Blink.
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

    // Setzt Materialfarbe kurz auf Hit-Farbe
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

    // Visualisiert Detection- und Attack-Range im Editor für schnelleres Tuning.
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
