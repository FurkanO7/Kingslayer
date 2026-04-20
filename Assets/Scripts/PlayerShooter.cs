using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class PlayerShooter : MonoBehaviour
{
    [Header("Input Actions")]
    [SerializeField] private InputActionReference shootAction;
    [SerializeField] private InputActionReference meleeAction;
    [SerializeField] private InputActionReference reloadAction;

    [Header("References")]
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private Projectile projectilePrefab;
    [SerializeField] private WeaponManager weaponManager;
    [SerializeField] private AudioSource shootAudioSource;

    [Header("Shot Settings")]
    [SerializeField] private float spawnDistanceFromCamera = 0.5f;
    
    [Header("Spherecast Settings")]
    [SerializeField] private float spherecastRadius = 0.5f;
    [SerializeField] private float spherecastDistance = 100f;

    [Header("Melee Settings")]
    [SerializeField] private float meleeRange = 2f;
    [SerializeField] private float meleeRadius = 1f;
    [SerializeField] private int meleeDamage = 35;
    [SerializeField] private float meleeCooldown = 0.45f;
    [SerializeField, Range(-1f, 1f)] private float meleeFrontThreshold = 0.35f;

    private float nextShotTime;
    private float nextMeleeTime;
    private bool isReloading;
    private float reloadEndTime;
    private float reloadStartTime;
    private float reloadDuration;
    private readonly HashSet<Collider> selfColliders = new HashSet<Collider>();

    public bool IsReloading => isReloading;
    public float ReloadProgress
    {
        get
        {
            if (!isReloading || reloadDuration <= 0f)
            {
                return 0f;
            }

            float elapsedTime = Time.time - reloadStartTime;
            return Mathf.Clamp01(elapsedTime / reloadDuration);
        }
    }

    private void Awake()
    {
        if (shootAudioSource == null)
        {
            shootAudioSource = GetComponent<AudioSource>();
        }

        CacheSelfColliders();
    }

    private void Update()
    {
        if (isReloading)
        {
            if (Time.time >= reloadEndTime)
            {
                CompleteReload();
            }
        }

        if (shootAction != null && shootAction.action.IsPressed())
        {
            TryShoot();
        }
    }

    private void OnEnable()
    {
        if (shootAction != null)
        {
            shootAction.action.Enable();
        }

        if (meleeAction != null)
        {
            meleeAction.action.Enable();
            meleeAction.action.performed += OnMeleePerformed;
        }

        if (reloadAction != null)
        {
            reloadAction.action.Enable();
            reloadAction.action.performed += OnReloadPerformed;
        }
    }

    private void OnDisable()
    {
        if (shootAction != null)
        {
            shootAction.action.Disable();
        }

        if (meleeAction != null)
        {
            meleeAction.action.performed -= OnMeleePerformed;
            meleeAction.action.Disable();
        }

        if (reloadAction != null)
        {
            reloadAction.action.performed -= OnReloadPerformed;
            reloadAction.action.Disable();
        }
    }

    private void OnMeleePerformed(InputAction.CallbackContext context)
    {
        TryMelee();
    }

    private void OnReloadPerformed(InputAction.CallbackContext context)
    {
        TryManualReload();
    }

    private void TryManualReload()
    {
        if (isReloading || weaponManager == null)
        {
            return;
        }

        Weapon equippedWeapon = weaponManager.EquippedWeapon;
        if (equippedWeapon == null || equippedWeapon.ReserveAmmo <= 0)
        {
            return;
        }

        if (equippedWeapon.AmmoInMagazine >= equippedWeapon.MagazineSize)
        {
            return;
        }

        isReloading = true;
        reloadStartTime = Time.time;
        reloadDuration = equippedWeapon.ReloadDuration;
        reloadEndTime = Time.time + equippedWeapon.ReloadDuration;
        PlayReloadSound(equippedWeapon);
    }

    private void TryShoot()
    {
        // Prüfe ob Waffe ausgerüstet ist
        if (weaponManager == null || !weaponManager.HasWeaponEquipped)
        {
            return;
        }

        Weapon equippedWeapon = weaponManager.EquippedWeapon;
        if (equippedWeapon == null)
        {
            return;
        }

        if (isReloading)
        {
            return;
        }

        if (!equippedWeapon.CanShoot)
        {
            StartReloadIfPossible(equippedWeapon);
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

        Vector3 baseDirection = cameraTransform.forward;
        Vector3 spawnPosition = cameraTransform.position + baseDirection * spawnDistanceFromCamera;

        EnemyAI hitEnemy = FindBestShootTarget(cameraTransform.position, baseDirection);
        if (hitEnemy != null)
        {
            hitEnemy.TakeDamage(equippedWeapon.ShotDamage);
        }

        // Projektil als Visualisierung instantiieren
        Quaternion spawnRotation = Quaternion.LookRotation(baseDirection, Vector3.up);
        Projectile projectile = Instantiate(projectilePrefab, spawnPosition, spawnRotation);
        projectile.Launch(baseDirection, equippedWeapon.ProjectileSpeed, gameObject.tag, transform.root);

        equippedWeapon.ConsumeShot();
        PlayShotSound(equippedWeapon);

        nextShotTime = Time.time + equippedWeapon.TimeBetweenShots;

        if (equippedWeapon.NeedsReload)
        {
            StartReloadIfPossible(equippedWeapon);
        }
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
        HashSet<EnemyAI> alreadyDamaged = new HashSet<EnemyAI>();
        EnemyAI[] enemies = FindObjectsByType<EnemyAI>(FindObjectsSortMode.None);

        foreach (EnemyAI enemy in enemies)
        {
            if (enemy == null)
            {
                continue;
            }

            Vector3 enemyPoint = enemy.AimPoint;
            float hitRadius = meleeRadius + enemy.HitRadius;
            if ((enemyPoint - attackCenter).sqrMagnitude > hitRadius * hitRadius)
            {
                continue;
            }

            Vector3 toTarget = (enemyPoint - cameraTransform.position).normalized;
            float forwardDot = Vector3.Dot(cameraTransform.forward, toTarget);
            if (forwardDot < meleeFrontThreshold)
            {
                continue;
            }

            if (alreadyDamaged.Contains(enemy))
            {
                continue;
            }

            enemy.TakeDamage(meleeDamage);
            alreadyDamaged.Add(enemy);
            Debug.Log($"Nahkampf-Treffer: {enemy.name}, Schaden: {meleeDamage}");
        }

        nextMeleeTime = Time.time + meleeCooldown;
    }

    private void StartReloadIfPossible(Weapon weapon)
    {
        if (weapon == null || isReloading)
        {
            return;
        }

        if (!weapon.NeedsReload)
        {
            if (!weapon.HasAnyAmmo)
            {
                Debug.Log("Keine Munition mehr.");
            }
            return;
        }

        isReloading = true;
        reloadStartTime = Time.time;
        reloadDuration = weapon.ReloadDuration;
        reloadEndTime = Time.time + weapon.ReloadDuration;
        PlayReloadSound(weapon);
        Debug.Log($"Reload gestartet ({weapon.ReloadDuration:0.0}s)");
    }

    private void CompleteReload()
    {
        isReloading = false;
        reloadDuration = 0f;

        if (weaponManager == null)
        {
            return;
        }

        Weapon equippedWeapon = weaponManager.EquippedWeapon;
        if (equippedWeapon == null)
        {
            return;
        }

        if (equippedWeapon.Reload())
        {
            Debug.Log($"Reload fertig. Munition: {equippedWeapon.AmmoInMagazine}/{equippedWeapon.ReserveAmmo}");
        }
    }

    private bool ShouldIgnoreHitCollider(Collider hitCollider)
    {
        if (hitCollider == null)
        {
            return true;
        }

        if (selfColliders.Contains(hitCollider))
        {
            return true;
        }

        if (hitCollider.GetComponent<Projectile>() != null)
        {
            return true;
        }

        if (hitCollider.GetComponentInParent<WeaponPickup>() != null)
        {
            return true;
        }

        return false;
    }

    private EnemyAI FindBestShootTarget(Vector3 startPosition, Vector3 direction)
    {
        EnemyAI[] enemies = FindObjectsByType<EnemyAI>(FindObjectsSortMode.None);
        EnemyAI bestEnemy = null;
        float bestDistance = spherecastDistance;

        foreach (EnemyAI enemy in enemies)
        {
            if (enemy == null)
            {
                continue;
            }

            Vector3 toEnemy = enemy.AimPoint - startPosition;
            float forwardDistance = Vector3.Dot(direction, toEnemy);
            if (forwardDistance < 0f || forwardDistance > spherecastDistance)
            {
                continue;
            }

            Vector3 closestPointOnRay = startPosition + direction * forwardDistance;
            float hitRadius = spherecastRadius + enemy.HitRadius;
            if ((enemy.AimPoint - closestPointOnRay).sqrMagnitude > hitRadius * hitRadius)
            {
                continue;
            }

            if (IsObstacleBlockingShot(startPosition, enemy.AimPoint, enemy))
            {
                continue;
            }

            if (forwardDistance < bestDistance)
            {
                bestDistance = forwardDistance;
                bestEnemy = enemy;
            }
        }

        return bestEnemy;
    }

    private bool IsObstacleBlockingShot(Vector3 startPosition, Vector3 targetPosition, EnemyAI targetEnemy)
    {
        Vector3 directionToTarget = targetPosition - startPosition;
        float distanceToTarget = directionToTarget.magnitude;
        if (distanceToTarget <= 0.001f)
        {
            return false;
        }

        RaycastHit[] hits = Physics.RaycastAll(startPosition, directionToTarget.normalized, distanceToTarget, ~0, QueryTriggerInteraction.Ignore);
        for (int i = 0; i < hits.Length; i++)
        {
            Collider hitCollider = hits[i].collider;
            if (ShouldIgnoreHitCollider(hitCollider))
            {
                continue;
            }

            EnemyAI hitEnemy = hitCollider.GetComponentInParent<EnemyAI>();
            if (hitEnemy == targetEnemy)
            {
                return false;
            }

            return true;
        }

        return false;
    }

    private void CacheSelfColliders()
    {
        selfColliders.Clear();

        Collider[] colliders = GetComponentsInChildren<Collider>(true);
        foreach (Collider col in colliders)
        {
            if (col != null)
            {
                selfColliders.Add(col);
            }
        }
    }

    private void PlayShotSound(Weapon weapon)
    {
        if (weapon == null || shootAudioSource == null || weapon.ShotSound == null)
        {
            return;
        }

        shootAudioSource.PlayOneShot(weapon.ShotSound, weapon.ShotVolume);
    }

    private void PlayReloadSound(Weapon weapon)
    {
        if (weapon == null || shootAudioSource == null || weapon.ReloadSound == null)
        {
            return;
        }

        shootAudioSource.PlayOneShot(weapon.ReloadSound, weapon.ReloadVolume);
    }
}
