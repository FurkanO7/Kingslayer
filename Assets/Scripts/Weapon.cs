using UnityEngine;

public class Weapon : MonoBehaviour
{
    [SerializeField] private string weaponName = "Waffe";
    [Header("Ammo")]
    [SerializeField] private int magazineSize = 5;
    [SerializeField] private int totalAmmo = 20;
    [SerializeField] private float reloadDuration = 1.5f;

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
    public bool CanShoot => ammoInMagazine > 0;
    public bool NeedsReload => ammoInMagazine <= 0 && reserveAmmo > 0;
    public bool HasAnyAmmo => (ammoInMagazine + reserveAmmo) > 0;

    private void Awake()
    {
        weaponColliders = GetComponents<Collider>();
        weaponRigidbody = GetComponent<Rigidbody>();
        InitializeAmmoIfNeeded();
    }
    
    public void OnEquipped()
    {
        InitializeAmmoIfNeeded();
        gameObject.SetActive(true);
        SetCarriedState();
    }
    
    public void OnUnequipped()
    {
        gameObject.SetActive(false);
    }

    public void PrepareForDrop()
    {
        gameObject.SetActive(true);
        SetDroppedState();
    }

    public bool ConsumeShot()
    {
        if (ammoInMagazine <= 0)
        {
            return false;
        }

        ammoInMagazine--;
        return true;
    }

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
