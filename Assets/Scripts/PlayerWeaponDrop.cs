using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerWeaponDrop : MonoBehaviour
{
    [SerializeField] private InputActionReference dropAction;
    [SerializeField] private WeaponManager weaponManager;

    // Registriert Events und aktiviert benoetigte Eingaben.
    private void OnEnable()
    {
        if (dropAction != null)
        {
            dropAction.action.Enable();
            dropAction.action.performed += OnDropPerformed;
        }
    }

    // Entfernt Event-Registrierungen und deaktiviert Eingaben.
    private void OnDisable()
    {
        if (dropAction != null)
        {
            dropAction.action.performed -= OnDropPerformed;
            dropAction.action.Disable();
        }
    }

    // Enthaelt die Logik fuer OnDropPerformed.
    private void OnDropPerformed(InputAction.CallbackContext context)
    {
        TryDropWeapon();
    }

    // Prueft Bedingungen und fuehrt DropWeapon nur bei Erfolg aus.
    private void TryDropWeapon()
    {
        if (weaponManager == null || !weaponManager.HasWeaponEquipped)
        {
            return;
        }

        Vector3 dropPosition = weaponManager.EquippedWeapon.transform.position;
        weaponManager.DropEquippedWeapon(dropPosition, Quaternion.identity);
    }
}
