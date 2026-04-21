using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponPickup : MonoBehaviour
{
    [SerializeField] private InputActionReference pickupAction;
    private string pickupPromptText = "WAFFE AUFNEHMEN";
    
    private Weapon weapon;
    private bool playerInRange;
    private Collider triggerCollider;

    // Initialisiert Referenzen und Startwerte.
    private void Awake()
    {
        CacheTriggerCollider();
    }
    
    // Registriert Events und aktiviert benoetigte Eingaben.
    private void OnEnable()
    {
        if (pickupAction != null)
        {
            pickupAction.action.Enable();
            pickupAction.action.performed += OnPickupPerformed;
        }
    }
    
    // Entfernt Event-Registrierungen und deaktiviert Eingaben.
    private void OnDisable()
    {
        if (pickupAction != null)
        {
            pickupAction.action.performed -= OnPickupPerformed;
            pickupAction.action.Disable();
        }
    }
    
    // Initialisiert Referenzen und Startzustand beim Szenenstart.
    private void Start()
    {
        weapon = GetComponent<Weapon>();
        if (weapon == null)
        {
            enabled = false;
            return;
        }
        
        CacheTriggerCollider();
    }
    
    // Reagiert auf Trigger-Eintritte.
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            string name = weapon != null ? weapon.WeaponName : pickupPromptText;
            if (PickupPromptHUD.Instance != null)
                PickupPromptHUD.Instance.Show(name);
        }
    }
    
    // Reagiert auf Trigger-Austritte.
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            if (PickupPromptHUD.Instance != null)
                PickupPromptHUD.Instance.Hide();
        }
    }
    
    // Enthaelt die Logik fuer OnPickupPerformed.
    private void OnPickupPerformed(InputAction.CallbackContext context)
    {
        if (playerInRange)
        {
            PickupWeapon();
        }
    }
    
    // Enthaelt die Logik fuer PickupWeapon.
    private void PickupWeapon()
    {
        WeaponManager weaponManager = FindFirstObjectByType<WeaponManager>();
        if (weaponManager != null)
        {
            weaponManager.EquipWeapon(weapon);
            
            // Finde den Player (parent von WeaponManager)
            Transform playerTransform = weaponManager.transform.parent ?? weaponManager.transform;
            Transform weaponHand = playerTransform.Find("WeaponHand");
            
            Transform parent = weaponHand != null ? weaponHand : playerTransform;
            transform.SetParent(parent);
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
            
            // Deaktiviere nur den Trigger-Collider fÃ¼r Pickup
            if (triggerCollider != null)
            {
                triggerCollider.enabled = false;
            }
            
            // Deaktiviere dieses Script
            enabled = false;
            if (PickupPromptHUD.Instance != null)
                PickupPromptHUD.Instance.Hide();
        }
        else
        {
        }
    }

    // Aktiviert PickupAfterDrop.
    public void EnablePickupAfterDrop()
    {
        playerInRange = false;
        CacheTriggerCollider();

        if (triggerCollider != null)
        {
            triggerCollider.enabled = true;
        }

        enabled = true;
    }

    // Speichert Referenzen fuer TriggerCollider zwischen.
    private void CacheTriggerCollider()
    {
        if (triggerCollider != null)
        {
            return;
        }

        // Finde den Trigger-Collider (Sphere/Capsule mit Is Trigger)
        foreach (Collider col in GetComponents<Collider>())
        {
            if (col.isTrigger)
            {
                triggerCollider = col;
                break;
            }
        }
    }
}
