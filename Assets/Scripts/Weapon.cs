using UnityEngine;

public class Weapon : MonoBehaviour
{
    [SerializeField] private string weaponName = "Waffe";
    [Header("Ammo")]
    [SerializeField] private int magazineSize = 5;
    [SerializeField] private int totalAmmo = 20;
    [SerializeField] private float reloadDuration = 1.5f;
    [Header("Firing")]
    [SerializeField] private int shotDamage = 20;
    [SerializeField, Min(0.01f)] private float timeBetweenShots = 0.5f;
    [Header("Audio")]
    [SerializeField] private AudioClip shotSound;
    [SerializeField, Range(0f, 1f)] private float shotVolume = 1f;
    [SerializeField] private AudioClip reloadSound;
    [SerializeField, Range(0f, 1f)] private float reloadVolume = 1f;
    [Header("Projectile")]
    [SerializeField, Min(1f)] private float projectileSpeed = 150f;

    private int ammoInMagazine;
    private int reserveAmmo;
    private bool ammoInitialized;
    private Collider[] weaponColliders;
    private Rigidbody weaponRigidbody;
    
    public string WeaponName => weaponName;
    public int MagazineSize => magazineSize;
    public int AmmoInMagazine => ammoInMagazine;
    public int ReserveAmmo => reserveAmmo;
    public float ReloadDuration => reloadDuration;
    public int ShotDamage => shotDamage;
    public float TimeBetweenShots => Mathf.Max(0.01f, timeBetweenShots);
    public AudioClip ShotSound => shotSound;
    public float ShotVolume => Mathf.Clamp01(shotVolume);
    public AudioClip ReloadSound => reloadSound;
    public float ReloadVolume => Mathf.Clamp01(reloadVolume);
    public float ProjectileSpeed => Mathf.Max(1f, projectileSpeed);
    public bool CanShoot => ammoInMagazine > 0;
    public bool NeedsReload => ammoInMagazine <= 0 && reserveAmmo > 0;
    public bool HasAnyAmmo => (ammoInMagazine + reserveAmmo) > 0;

    // Initialisiert Referenzen und Startwerte.
    private void Awake()
    {
        weaponColliders = GetComponents<Collider>();
        weaponRigidbody = GetComponent<Rigidbody>();
        InitializeAmmoIfNeeded();
    }
    
    // Enthaelt die Logik fuer OnEquipped.
    public void OnEquipped()
    {
        InitializeAmmoIfNeeded();
        gameObject.SetActive(true);
        SetCarriedState();
    }
    
    // Enthaelt die Logik fuer OnUnequipped.
    public void OnUnequipped()
    {
        gameObject.SetActive(false);
    }

    // Enthaelt die Logik fuer PrepareForDrop.
    public void PrepareForDrop()
    {
        gameObject.SetActive(true);
        SetDroppedState();
    }

    // Enthaelt die Logik fuer ConsumeShot.
    public bool ConsumeShot()
    {
        if (ammoInMagazine <= 0)
        {
            return false;
        }

        ammoInMagazine--;
        return true;
    }

    // Enthaelt die Logik fuer Reload.
    public bool Reload()
    {
        if (reserveAmmo <= 0)
        {
            return false;
        }

        if (ammoInMagazine >= magazineSize)
        {
            return false;
        }

        int missingInMagazine = magazineSize - ammoInMagazine;
        int ammoToLoad = Mathf.Min(missingInMagazine, reserveAmmo);
        ammoInMagazine += ammoToLoad;
        reserveAmmo -= ammoToLoad;
        return ammoToLoad > 0;
    }

    // Enthaelt die Logik fuer AddReserveAmmo.
    public int AddReserveAmmo(int amount)
    {
        if (amount <= 0)
        {
            return 0;
        }

        InitializeAmmoIfNeeded();
        reserveAmmo += amount;
        return amount;
    }

    // Enthaelt die Logik fuer InitializeAmmoIfNeeded.
    private void InitializeAmmoIfNeeded()
    {
        if (ammoInitialized)
        {
            return;
        }

        if (magazineSize < 1)
        {
            magazineSize = 1;
        }

        if (totalAmmo < 0)
        {
            totalAmmo = 0;
        }

        ammoInMagazine = Mathf.Min(magazineSize, totalAmmo);
        reserveAmmo = Mathf.Max(0, totalAmmo - ammoInMagazine);
        ammoInitialized = true;
    }

    // Setzt den Wert oder Zustand fuer CarriedState.
    private void SetCarriedState()
    {
        EnsureRigidbody();

        if (weaponRigidbody != null)
        {
            weaponRigidbody.linearVelocity = Vector3.zero;
            weaponRigidbody.angularVelocity = Vector3.zero;
            weaponRigidbody.useGravity = false;
            weaponRigidbody.isKinematic = true;
            weaponRigidbody.detectCollisions = false;
            weaponRigidbody.interpolation = RigidbodyInterpolation.None;
            weaponRigidbody.collisionDetectionMode = CollisionDetectionMode.Discrete;
        }

        SetSolidCollidersEnabled(false);
    }

    // Setzt den Wert oder Zustand fuer DroppedState.
    private void SetDroppedState()
    {
        EnsureRigidbody();

        if (weaponRigidbody != null)
        {
            weaponRigidbody.linearVelocity = Vector3.zero;
            weaponRigidbody.angularVelocity = Vector3.zero;
            weaponRigidbody.useGravity = true;
            weaponRigidbody.isKinematic = false;
            weaponRigidbody.detectCollisions = true;
            weaponRigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            weaponRigidbody.interpolation = RigidbodyInterpolation.Interpolate;
        }

        SetSolidCollidersEnabled(true);
    }

    // Setzt den Wert oder Zustand fuer SolidCollidersEnabled.
    private void SetSolidCollidersEnabled(bool enabled)
    {
        if (weaponColliders == null)
        {
            return;
        }

        for (int i = 0; i < weaponColliders.Length; i++)
        {
            Collider col = weaponColliders[i];
            if (col == null || col.isTrigger)
            {
                continue;
            }

            col.enabled = enabled;
        }
    }

    // Stellt sicher, dass Rigidbody vorhanden ist.
    private void EnsureRigidbody()
    {
        if (weaponRigidbody != null)
        {
            return;
        }

        weaponRigidbody = GetComponent<Rigidbody>();
        if (weaponRigidbody == null)
        {
            weaponRigidbody = gameObject.AddComponent<Rigidbody>();
        }
    }
}
