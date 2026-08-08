using UnityEngine;

public class Shell_Behavior : MonoBehaviour
{
    [Header("Components")]
    public Transform shell_transform;
    public Rigidbody shell_rigidbody;
    public CapsuleCollider shell_capsuleCollider;

    [Header("Shell Parameters")]
    public float getDamageMultiplier;   // Damage multiplier for this shell




    // Start is called before the first frame update
    void Start()
    {
        
        // Initialize Shell's Rigidbody.
        shell_rigidbody = GetComponent<Rigidbody>();
        shell_rigidbody.mass = 100f;
        shell_rigidbody.drag = 1f;
        shell_rigidbody.angularDrag = 0.05f;
        shell_rigidbody.isKinematic = false;
        shell_rigidbody.useGravity = true;
        shell_rigidbody.constraints = 
        (
            RigidbodyConstraints.None
        );

        // Initialize Shell's Capsule Collider.
        shell_capsuleCollider = GetComponent<CapsuleCollider>();
        shell_capsuleCollider.enabled     = true;
        shell_capsuleCollider.isTrigger   = false;
        shell_capsuleCollider.providesContacts = false;
        shell_capsuleCollider.center      = new Vector3(0f, 0f, 0.15f);
        shell_capsuleCollider.radius      = 0.15f;
        shell_capsuleCollider.height      = 0.65f;
        shell_capsuleCollider.direction   = 2; // Z-axis
        
    }

    // Start of my Functions.==============================

    private void OnCollisionEnter(Collision collision)
    {
        Destroy(gameObject);
        Debug.Log("Shell collided with " + collision.gameObject.name);

        Health_System healthSystem = collision.gameObject.GetComponent<Health_System>();
        if (healthSystem != null)
        {
            healthSystem.TakeDamage(getDamageMultiplier); // Apply damage with a multiplier of 1.0 (can be adjusted).
        }

        // Later we will add explosion effects, damage calculation, etc.
    }

    // End of my Functions.================================

    // Update is called once per frame
    void Update()
    {
        
    }
}
