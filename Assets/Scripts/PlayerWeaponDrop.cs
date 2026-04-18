using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerWeaponDrop : MonoBehaviour
{
    [SerializeField] private InputActionReference dropAction;
    [SerializeField] private WeaponManager weaponManager;
    [SerializeField] private Transform dropOrigin;
    [SerializeField] private float dropDistance = 1.2f;
    [SerializeField] private float dropHeightOffset = 0.6f;

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

        Transform origin = dropOrigin != null ? dropOrigin : transform;
        Vector3 forward = origin.forward;
        forward.y = 0f;
        if (forward.sqrMagnitude < 0.01f)
        {
            forward = transform.forward;
            forward.y = 0f;
        }

        forward.Normalize();

        Vector3 dropPosition = origin.position + forward * dropDistance + Vector3.up * dropHeightOffset;
        Quaternion dropRotation = Quaternion.LookRotation(forward, Vector3.up);

        weaponManager.DropEquippedWeapon(dropPosition, dropRotation);
    }
}
