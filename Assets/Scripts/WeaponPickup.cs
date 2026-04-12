using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponPickup : MonoBehaviour
{
    [SerializeField] private InputActionReference pickupAction;
    [SerializeField] private string pickupPromptText = "WAFFE AUFNEHMEN";
    
    private Weapon weapon;
    private bool playerInRange;
    private Collider triggerCollider;
    
    private void OnEnable()
    {
        if (pickupAction != null)
        {
            pickupAction.action.Enable();
            pickupAction.action.performed += OnPickupPerformed;
        }
    }
    
    private void OnDisable()
    {
        if (pickupAction != null)
        {
            pickupAction.action.performed -= OnPickupPerformed;
            pickupAction.action.Disable();
        }
    }
    
    private void Start()
    {
        weapon = GetComponent<Weapon>();
        if (weapon == null)
        {
            Debug.LogError("WeaponPickup: Weapon Script nicht auf diesem Objekt gefunden!");
            enabled = false;
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
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            Debug.Log(pickupPromptText);
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
        }
    }
    
    private void OnPickupPerformed(InputAction.CallbackContext context)
    {
        if (playerInRange)
        {
            PickupWeapon();
        }
    }
    
    private void PickupWeapon()
    {
        WeaponManager weaponManager = FindObjectOfType<WeaponManager>();
        if (weaponManager != null)
        {
            weaponManager.EquipWeapon(weapon);
            
            // Finde den Player (parent von WeaponManager)
            Transform playerTransform = weaponManager.transform.parent ?? weaponManager.transform;
            Transform weaponHand = playerTransform.Find("WeaponHand");
            
            if (weaponHand != null)
            {
                transform.SetParent(weaponHand);
                transform.localPosition = Vector3.zero;
                transform.localRotation = Quaternion.identity;
                Debug.Log("Waffe erfolgreich zu WeaponHand moved!");
            }
            else
            {
                Debug.LogWarning($"WeaponPickup: WeaponHand nicht gefunden! Suche in: {playerTransform.name}");
                Debug.LogWarning($"Gefundene Kinder: {string.Join(", ", System.Linq.Enumerable.Select(playerTransform.GetComponentsInChildren<Transform>(), t => t.name))}");
                transform.SetParent(playerTransform);
                transform.localPosition = Vector3.zero;
                transform.localRotation = Quaternion.identity;
            }
            
            // Deaktiviere nur den Trigger-Collider für Pickup
            if (triggerCollider != null)
            {
                triggerCollider.enabled = false;
            }
            
            // Deaktiviere dieses Script
            enabled = false;
        }
        else
        {
            Debug.LogError("WeaponPickup: WeaponManager nicht gefunden!");
        }
    }
}
