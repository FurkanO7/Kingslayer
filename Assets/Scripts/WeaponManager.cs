using UnityEngine;

public class WeaponManager : MonoBehaviour
{
    private Weapon equippedWeapon;
    
    public Weapon EquippedWeapon => equippedWeapon;
    public bool HasWeaponEquipped => equippedWeapon != null;
    
    public void EquipWeapon(Weapon weapon)
    {
        if (weapon == null)
        {
            Debug.LogWarning("Versucht NULL-Waffe auszurüsten");
            return;
        }
        
        // Alte Waffe ablegen
        if (equippedWeapon != null)
        {
            equippedWeapon.OnUnequipped();
        }
        
        // Neue Waffe ausrüsten
        equippedWeapon = weapon;
        equippedWeapon.OnEquipped();
        Debug.Log($"Waffe ausgerüstet: {weapon.WeaponName}");
    }
    
    public void UnequipWeapon()
    {
        if (equippedWeapon != null)
        {
            equippedWeapon.OnUnequipped();
            Debug.Log($"Waffe abgelegt: {equippedWeapon.WeaponName}");
            equippedWeapon = null;
        }
    }

    public bool DropEquippedWeapon(Vector3 worldPosition, Quaternion worldRotation)
    {
        if (equippedWeapon == null)
        {
            return false;
        }

        Weapon weaponToDrop = equippedWeapon;
        equippedWeapon = null;

        Transform weaponTransform = weaponToDrop.transform;
        weaponTransform.SetParent(null, true);
        weaponTransform.SetPositionAndRotation(worldPosition, worldRotation);
        weaponToDrop.gameObject.SetActive(true);

        Rigidbody rb = weaponToDrop.GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = weaponToDrop.gameObject.AddComponent<Rigidbody>();
        }

        rb.isKinematic = false;
        rb.useGravity = true;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        WeaponPickup pickup = weaponToDrop.GetComponent<WeaponPickup>();
        if (pickup != null)
        {
            pickup.EnablePickupAfterDrop();
        }

        Debug.Log($"Waffe gedroppt: {weaponToDrop.WeaponName}");
        return true;
    }
}
