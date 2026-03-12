using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerShooter : MonoBehaviour
{
    [Header("Input Actions")]
    [SerializeField] private InputActionReference shootAction;

    [Header("References")]
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private Projectile projectilePrefab;

    [Header("Shot Settings")]
    [SerializeField] private float projectileSpeed = 70f;
    [SerializeField] private float shotCooldown = 0.5f;
    [SerializeField] private float spawnDistanceFromCamera = 0.5f;

    private float nextShotTime;

    private void OnEnable()
    {
        if (shootAction != null)
        {
            shootAction.action.Enable();
            shootAction.action.performed += OnShootPerformed;
        }
    }

    private void OnDisable()
    {
        if (shootAction != null)
        {
            shootAction.action.performed -= OnShootPerformed;
            shootAction.action.Disable();
        }
    }

    private void OnShootPerformed(InputAction.CallbackContext context)
    {
        TryShoot();
    }

    private void TryShoot()
    {
        if (Time.time < nextShotTime)
        {
            return;
        }

        if (cameraTransform == null || projectilePrefab == null)
        {
            return;
        }

        Vector3 spawnPosition = cameraTransform.position + cameraTransform.forward * spawnDistanceFromCamera;
        Quaternion spawnRotation = Quaternion.LookRotation(cameraTransform.forward, Vector3.up);

        Projectile projectile = Instantiate(projectilePrefab, spawnPosition, spawnRotation);
        projectile.Launch(cameraTransform.forward, projectileSpeed, gameObject.tag, transform.root);

        nextShotTime = Time.time + shotCooldown;
    }
}
