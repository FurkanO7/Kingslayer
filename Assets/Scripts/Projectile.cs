using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class Projectile : MonoBehaviour
{
    [SerializeField] private float lifetime = 4f;

    private Rigidbody rb;
    private Collider projectileCollider;
    private string ownerTag;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        projectileCollider = GetComponent<Collider>();
    }

    private void OnEnable()
    {
        Destroy(gameObject, lifetime);
    }

    public void Launch(Vector3 direction, float speed, string sourceTag, Transform sourceRoot)
    {
        ownerTag = sourceTag;

        if (sourceRoot != null && projectileCollider != null)
        {
            Collider[] sourceColliders = sourceRoot.GetComponentsInChildren<Collider>();
            foreach (Collider sourceCollider in sourceColliders)
            {
                Physics.IgnoreCollision(projectileCollider, sourceCollider, true);
            }
        }

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

        if (ShouldDestroyOnTag(hitCollider.tag))
        {
            Destroy(gameObject);
        }
    }

    private bool ShouldDestroyOnTag(string hitTag)
    {
        if (hitTag == "obstacle")
        {
            return true;
        }

        if (ownerTag == "Player" && hitTag == "Enemy")
        {
            return true;
        }

        if (ownerTag == "Enemy" && hitTag == "Player")
        {
            return true;
        }

        return false;
    }
}
