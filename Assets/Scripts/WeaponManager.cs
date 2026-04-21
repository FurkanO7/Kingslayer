using UnityEngine;

public class WeaponManager : MonoBehaviour
{
    private Weapon equippedWeapon;
    
    public Weapon EquippedWeapon => equippedWeapon;
    public bool HasWeaponEquipped => equippedWeapon != null;
    
    // Enthaelt die Logik fuer EquipWeapon.
    public void EquipWeapon(Weapon weapon)
    {
        if (weapon == null)
        {
            return;
        }
        
        // Alte Waffe ablegen
        if (equippedWeapon != null)
        {
            equippedWeapon.OnUnequipped();
        }
        
        // Neue Waffe ausrÃ¼sten
        equippedWeapon = weapon;
        equippedWeapon.OnEquipped();
    }
    
    // Enthaelt die Logik fuer UnequipWeapon.
    public void UnequipWeapon()
    {
        if (equippedWeapon != null)
        {
            equippedWeapon.OnUnequipped();
            equippedWeapon = null;
        }
    }

    // Enthaelt die Logik fuer DropEquippedWeapon.
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
        weaponToDrop.PrepareForDrop();

        WeaponPickup pickup = weaponToDrop.GetComponent<WeaponPickup>();
        if (pickup != null)
        {
            pickup.EnablePickupAfterDrop();
        }

        return true;
    }
}
