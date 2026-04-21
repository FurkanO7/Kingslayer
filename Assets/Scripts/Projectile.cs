using UnityEngine;

/// <summary>
/// Reine Visualisierung eines Projektils.
/// Der eigentliche Hit & Damage wird via Spherecast in PlayerShooter gemacht.
/// Dieses Skript dient nur dazu, das Projektil fliegen zu lassen und zu zerstÃ¶ren.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class Projectile : MonoBehaviour
{
    [SerializeField] private float lifetime = 6f;

    private Rigidbody rb;
    private string ownerTag;
    private Transform ownerRoot;
    public string OwnerTag => ownerTag;

    // Initialisiert Referenzen und Startwerte.
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Registriert Events und aktiviert benoetigte Eingaben.
    private void OnEnable()
    {
        Destroy(gameObject, lifetime);
    }

    // Enthaelt die Logik fuer Launch.
    public void Launch(Vector3 direction, float speed, string sourceTag, Transform sourceRoot)
    {
        ownerTag = sourceTag;
        ownerRoot = sourceRoot;

        Vector3 normalizedDirection = direction.normalized;
        rb.linearVelocity = normalizedDirection * speed;
    }

    // Reagiert auf Kollisionen mit anderen Objekten.
    private void OnCollisionEnter(Collision collision)
    {
        HandleHit(collision.collider);
    }

    // Reagiert auf Trigger-Eintritte.
    private void OnTriggerEnter(Collider other)
    {
        HandleHit(other);
    }

    // Verarbeitet Hit.
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
