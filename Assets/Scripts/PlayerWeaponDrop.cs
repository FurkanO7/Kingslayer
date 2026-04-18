using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerWeaponDrop : MonoBehaviour
{
    [SerializeField] private InputActionReference dropAction;
    [SerializeField] private WeaponManager weaponManager;

    private void OnEnable()
    {
        if (dropAction != null)
        {
            dropAction.action.Enable();
            dropAction.action.performed += OnDropPerformed;
        }
    }

    private void OnDisable()
    {
        if (dropAction != null)
        {
            dropAction.action.performed -= OnDropPerformed;
            dropAction.action.Disable();
        }
    }

    private void OnDropPerformed(InputAction.CallbackContext context)
    {
        TryDropWeapon();
    }

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
