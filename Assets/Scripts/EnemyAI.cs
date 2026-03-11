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
    [SerializeField] private float detectionRange = 20f;
    [SerializeField] private float attackRange = 12f;
    [SerializeField] private float repathInterval = 0.2f;

    [Header("Shooting")]
    [SerializeField] private float projectileSpeed = 25f;
    [SerializeField] private float timeBetweenShots = 0.25f;
    [SerializeField] private int minBurstShots = 2;
    [SerializeField] private int maxBurstShots = 3;

    [Header("Reposition")]
    [SerializeField] private float repositionMinDistance = 2.5f;
    [SerializeField] private float repositionMaxDistance = 4.5f;
    [SerializeField] private float repositionStopDistance = 0.5f;

    [Header("Search Movement")]
    [SerializeField] private float searchRadius = 8f;
    [SerializeField] private float searchPointTolerance = 0.8f;

    private NavMeshAgent agent;
    private EnemyState state;
    private float nextRepathTime;
    private Vector3 searchCenter;
    private bool burstRoutineRunning;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        searchCenter = transform.position;
        state = EnemyState.Searching;
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

    private void UpdateSearching()
    {
        if (CanSeePlayerInDetectionRange())
        {
            state = EnemyState.Chasing;
            return;
        }

        if (!CanUseAgent())
        {
            return;
        }

        if (!agent.pathPending && agent.remainingDistance <= searchPointTolerance)
        {
            Vector3 randomPoint = searchCenter + Random.insideUnitSphere * searchRadius;
            if (TryGetNavMeshPoint(randomPoint, searchRadius, out Vector3 navPoint))
            {
                agent.SetDestination(navPoint);
            }
        }
    }

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

        if (!burstRoutineRunning)
        {
            StartCoroutine(FireBurstRoutine());
        }
    }

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

    private bool CanSeePlayerInDetectionRange()
    {
        if (playerTransform == null)
        {
            return false;
        }

        return Vector3.Distance(transform.position, playerTransform.position) <= detectionRange;
    }

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

    private bool CanUseAgent()
    {
        return agent != null && agent.isActiveAndEnabled && agent.isOnNavMesh;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
