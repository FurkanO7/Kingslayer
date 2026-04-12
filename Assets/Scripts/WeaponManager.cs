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
}
