using UnityEngine;

public class PlayerCrosshairHUD : MonoBehaviour
{
    [SerializeField] private WeaponManager weaponManager;
    [SerializeField] private GameObject defaultCrosshair;
    private bool hadWeaponLastFrame;

    private void Update()
    {
        RefreshCrosshair();
    }

    public void SetWeaponManager(WeaponManager newWeaponManager)
    {
        weaponManager = newWeaponManager;
        hadWeaponLastFrame = false;
        RefreshCrosshair();
    }

    private void RefreshCrosshair()
    {
        bool hasWeapon = weaponManager != null && weaponManager.HasWeaponEquipped && weaponManager.EquippedWeapon != null;

        if (hasWeapon == hadWeaponLastFrame)
        {
            return;
        }

        if (defaultCrosshair != null)
        {
            defaultCrosshair.SetActive(hasWeapon);
        }

        hadWeaponLastFrame = hasWeapon;
    }
}
