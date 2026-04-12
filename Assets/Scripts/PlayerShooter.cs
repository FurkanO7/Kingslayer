using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerShooter : MonoBehaviour
{
    [Header("Input Actions")]
    [SerializeField] private InputActionReference shootAction;
    [SerializeField] private InputActionReference meleeAction;

    [Header("References")]
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private Projectile projectilePrefab;
    [SerializeField] private WeaponManager weaponManager;

    [Header("Shot Settings")]
    [SerializeField] private float projectileSpeed = 150f;
    [SerializeField] private float shotCooldown = 0.5f;
    [SerializeField] private float spawnDistanceFromCamera = 0.5f;
    
    [Header("Spherecast Settings")]
    [SerializeField] private float spherecastRadius = 0.5f;
    [SerializeField] private float spherecastDistance = 100f;
    [SerializeField] private int projectileDamage = 20;
    [SerializeField] private LayerMask hitMask = -1;

    [Header("Melee Settings")]
    [SerializeField] private float meleeRange = 2f;
    [SerializeField] private float meleeRadius = 1f;
    [SerializeField] private int meleeDamage = 35;
    [SerializeField] private float meleeCooldown = 0.45f;
    [SerializeField, Range(-1f, 1f)] private float meleeFrontThreshold = 0.35f;

    private float nextShotTime;
    private float nextMeleeTime;

    private void OnEnable()
    {
        if (shootAction != null)
        {
            shootAction.action.Enable();
            shootAction.action.performed += OnShootPerformed;
        }

        if (meleeAction != null)
        {
            meleeAction.action.Enable();
            meleeAction.action.performed += OnMeleePerformed;
        }
    }

    private void OnDisable()
    {
        if (shootAction != null)
        {
            shootAction.action.performed -= OnShootPerformed;
            shootAction.action.Disable();
        }

        if (meleeAction != null)
        {
            meleeAction.action.performed -= OnMeleePerformed;
            meleeAction.action.Disable();
        }
    }

    private void OnShootPerformed(InputAction.CallbackContext context)
    {
        TryShoot();
    }

    private void OnMeleePerformed(InputAction.CallbackContext context)
    {
        TryMelee();
    }

    private void TryShoot()
    {
        // Prüfe ob Waffe ausgerüstet ist
        if (weaponManager == null || !weaponManager.HasWeaponEquipped)
        {
            return;
        }

        if (Time.time < nextShotTime)
        {
            return;
        }

        if (cameraTransform == null || projectilePrefab == null)
        {
            return;
        }

        Vector3 shootDirection = cameraTransform.forward;

        // Spherecast startet direkt von der Kamera (nicht vorne versetzt!)
        PerformSpherecasting(cameraTransform.position, shootDirection);

        // Projektil als Visualisierung instantiieren
        Vector3 spawnPosition = cameraTransform.position + shootDirection * spawnDistanceFromCamera;
        Quaternion spawnRotation = Quaternion.LookRotation(shootDirection, Vector3.up);
        Projectile projectile = Instantiate(projectilePrefab, spawnPosition, spawnRotation);
        projectile.Launch(shootDirection, projectileSpeed, gameObject.tag, transform.root);

        nextShotTime = Time.time + shotCooldown;
    }

    private void TryMelee()
    {
        if (Time.time < nextMeleeTime)
        {
            return;
        }

        if (cameraTransform == null)
        {
            return;
        }

        Vector3 attackCenter = cameraTransform.position + (cameraTransform.forward * meleeRange);
        Collider[] hits = Physics.OverlapSphere(attackCenter, meleeRadius, hitMask);
        System.Collections.Generic.HashSet<EnemyAI> alreadyDamaged = new System.Collections.Generic.HashSet<EnemyAI>();

        foreach (Collider hit in hits)
        {
            if (hit == null || hit.CompareTag("Player"))
            {
                continue;
            }

            Vector3 toTarget = (hit.bounds.center - cameraTransform.position).normalized;
            float forwardDot = Vector3.Dot(cameraTransform.forward, toTarget);
            if (forwardDot < meleeFrontThreshold)
            {
                continue;
            }

            EnemyAI enemy = hit.GetComponent<EnemyAI>();
            if (enemy == null)
            {
                enemy = hit.GetComponentInParent<EnemyAI>();
            }

            if (enemy == null || alreadyDamaged.Contains(enemy))
            {
                continue;
            }

            enemy.TakeDamage(meleeDamage);
            alreadyDamaged.Add(enemy);
            Debug.Log($"Nahkampf-Treffer: {enemy.name}, Schaden: {meleeDamage}");
        }

        nextMeleeTime = Time.time + meleeCooldown;
    }

    private void PerformSpherecasting(Vector3 startPosition, Vector3 direction)
    {
        System.Collections.Generic.HashSet<Collider> alreadyHit = new System.Collections.Generic.HashSet<Collider>();

        // OverlapSphere am Startpunkt: fängt Gegner ab die direkt in Reichweite sind
        // (SphereCastAll ignoriert Collider die bereits am Startpunkt überlappen)
        Collider[] overlapping = Physics.OverlapSphere(startPosition, spherecastRadius, hitMask);
        foreach (Collider col in overlapping)
        {
            if (!col.CompareTag("Player"))
            {
                ProcessHit(col);
                alreadyHit.Add(col);
            }
        }

        // SphereCastAll für alles weiter entfernt
        RaycastHit[] hits = Physics.SphereCastAll(startPosition, spherecastRadius, direction, spherecastDistance, hitMask);
        foreach (RaycastHit hit in hits)
        {
            if (alreadyHit.Contains(hit.collider))
            {
                continue;
            }

            if (!hit.collider.CompareTag("Player"))
            {
                ProcessHit(hit.collider);
                alreadyHit.Add(hit.collider);
            }
        }
    }

    private void ProcessHit(Collider hitCollider)
    {
        if (hitCollider.CompareTag("Enemy"))
        {
            EnemyAI enemy = hitCollider.GetComponent<EnemyAI>();
            if (enemy == null)
            {
                enemy = hitCollider.GetComponentInParent<EnemyAI>();
            }
            if (enemy != null)
            {
                enemy.TakeDamage(projectileDamage);
                Debug.Log($"Gegner getroffen: {hitCollider.name}, Schaden: {projectileDamage}");
            }
        }
    }
}
