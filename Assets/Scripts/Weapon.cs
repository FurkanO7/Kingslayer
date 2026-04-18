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
        InitializeAmmoIfNeeded();
    }
    
    public void OnEquipped()
    {
        InitializeAmmoIfNeeded();
        // Waffe aktivieren wenn ausgerüstet
        gameObject.SetActive(true);
    }
    
    public void OnUnequipped()
    {
        // Waffe deaktivieren wenn abgelegt
        gameObject.SetActive(false);
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
}
