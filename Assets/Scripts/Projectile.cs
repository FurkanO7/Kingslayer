using UnityEngine;

/// <summary>
/// Reine Visualisierung eines Projektils.
/// Der eigentliche Hit & Damage wird via Spherecast in PlayerShooter gemacht.
/// Dieses Skript dient nur dazu, das Projektil fliegen zu lassen und zu zerstören.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class Projectile : MonoBehaviour
{
    [SerializeField] private float lifetime = 6f;

    private Rigidbody rb;
    private string ownerTag;
    private Transform ownerRoot;
    public string OwnerTag => ownerTag;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void OnEnable()
    {
        Destroy(gameObject, lifetime);
    }

    public void Launch(Vector3 direction, float speed, string sourceTag, Transform sourceRoot)
    {
        ownerTag = sourceTag;
        ownerRoot = sourceRoot;

        Vector3 normalizedDirection = direction.normalized;
        rb.linearVelocity = normalizedDirection * speed;
    }

    private void OnCollisionEnter(Collision collision)
    {
        HandleHit(collision.collider);
    }

    private void OnTriggerEnter(Collider other)
    {
        HandleHit(other);
    }

    private void HandleHit(Collider hitCollider)
    {
        if (hitCollider == null)
        {
            return;
        }

        if (ownerRoot != null && hitCollider.transform.root == ownerRoot)
        {
            return;
        }

        Destroy(gameObject);
    }
}
