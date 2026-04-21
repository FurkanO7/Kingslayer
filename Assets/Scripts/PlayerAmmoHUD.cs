using UnityEngine;
using TMPro;

public class PlayerAmmoHUD : MonoBehaviour
{
    [SerializeField] private WeaponManager weaponManager;
    [SerializeField] private TMP_Text ammoText;
    [SerializeField] private string noWeaponText = "- / -";
    [SerializeField] private bool includeWeaponName;

    private Weapon lastWeapon;
    private int lastMagazine = -1;
    private int lastReserve = -1;

    // Aktualisiert die Logik in jedem Frame.
    private void Update()
    {
        RefreshIfChanged();
    }

    // Setzt den Wert oder Zustand fuer WeaponManager.
    public void SetWeaponManager(WeaponManager newWeaponManager)
    {
        weaponManager = newWeaponManager;
        lastWeapon = null;
        lastMagazine = -1;
        lastReserve = -1;
        RefreshIfChanged();
    }

    // Aktualisiert IfChanged.
    private void RefreshIfChanged()
    {
        if (ammoText == null)
        {
            return;
        }

        if (weaponManager == null || !weaponManager.HasWeaponEquipped)
        {
            if (lastWeapon != null || ammoText.text != noWeaponText)
            {
                ammoText.text = noWeaponText;
                lastWeapon = null;
                lastMagazine = -1;
                lastReserve = -1;
            }
            return;
        }

        Weapon weapon = weaponManager.EquippedWeapon;
        if (weapon == null)
        {
            if (lastWeapon != null || ammoText.text != noWeaponText)
            {
                ammoText.text = noWeaponText;
                lastWeapon = null;
                lastMagazine = -1;
                lastReserve = -1;
            }
            return;
        }

        int currentMagazine = weapon.AmmoInMagazine;
        int currentReserve = weapon.ReserveAmmo;

        if (weapon == lastWeapon && currentMagazine == lastMagazine && currentReserve == lastReserve)
        {
            return;
        }

        if (includeWeaponName)
        {
            ammoText.text = weapon.WeaponName + " " + currentMagazine + " / " + currentReserve;
        }
        else
        {
            ammoText.text = currentMagazine + " / " + currentReserve;
        }

        lastWeapon = weapon;
        lastMagazine = currentMagazine;
        lastReserve = currentReserve;
    }
}
