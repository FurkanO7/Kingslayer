using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerShooter : MonoBehaviour
{
    [Header("Input Actions")]
    [SerializeField] private InputActionReference shootAction;

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

    private float nextShotTime;

    private void OnEnable()
    {
        if (shootAction != null)
        {
            shootAction.action.Enable();
            shootAction.action.performed += OnShootPerformed;
        }
    }

    private void OnDisable()
    {
        if (shootAction != null)
        {
            shootAction.action.performed -= OnShootPerformed;
            shootAction.action.Disable();
        }
    }

    private void OnShootPerformed(InputAction.CallbackContext context)
    {
        TryShoot();
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
