using UnityEngine;

public class AmmoBox : MonoBehaviour
{
    [Header("Ammo Settings")]
    [SerializeField] private int ammoAmount = 10;
    [SerializeField] private bool consumeOnUse = true;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        WeaponManager weaponManager = other.GetComponent<WeaponManager>();
        if (weaponManager == null)
        {
            weaponManager = other.GetComponentInChildren<WeaponManager>();
        }

        if (weaponManager == null)
        {
            weaponManager = other.GetComponentInParent<WeaponManager>();
        }

        if (weaponManager == null || !weaponManager.HasWeaponEquipped)
        {
            return;
        }

        Weapon equippedWeapon = weaponManager.EquippedWeapon;
        if (equippedWeapon == null)
        {
            return;
        }

        int grantedAmmo = equippedWeapon.AddReserveAmmo(ammoAmount);
        if (grantedAmmo <= 0)
        {
            return;
        }

        Debug.Log($"AmmoBox aufgehoben: +{grantedAmmo} Munition");

        if (consumeOnUse)
        {
            Destroy(gameObject);
        }
    }
}
