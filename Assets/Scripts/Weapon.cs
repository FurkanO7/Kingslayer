using UnityEngine;

public class Weapon : MonoBehaviour
{
    [SerializeField] private string weaponName = "Waffe";
    
    public string WeaponName => weaponName;
    
    public void OnEquipped()
    {
        // Waffe aktivieren wenn ausgerüstet
        gameObject.SetActive(true);
    }
    
    public void OnUnequipped()
    {
        // Waffe deaktivieren wenn abgelegt
        gameObject.SetActive(false);
    }
}
