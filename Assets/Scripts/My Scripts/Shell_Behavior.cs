using UnityEngine;

public class Shell_Behavior : MonoBehaviour
{

    public Transform shell_transform;
    public Rigidbody shell_rigidbody;
    public CapsuleCollider shell_capsuleCollider;

    [Header("Shell Parameters")]
    public float getDamageMultiplier;           // Damage multiplier for this shell.
    public float lifeTime = 10f;                // Lifetime of the shell in case it does not collide with anything.
    private Collider currentShooterCollider;    // Variable to remember who is shooting the shell.

    void Awake()
    {
        
        // Initialize Shell's Rigidbody.
        shell_rigidbody = GetComponent<Rigidbody>();
        shell_rigidbody.isKinematic = false;
        shell_rigidbody.useGravity = true;
        shell_rigidbody.constraints = 
        (
            RigidbodyConstraints.None
        );

        // Initialize Shell's Capsule Collider.
        shell_capsuleCollider = GetComponent<CapsuleCollider>();
        /*
        shell_capsuleCollider.enabled     = true;
        shell_capsuleCollider.isTrigger   = false;
        shell_capsuleCollider.center      = new Vector3(0f, 0f, 0.15f);
        shell_capsuleCollider.radius      = 0.15f;
        shell_capsuleCollider.height      = 0.65f;
        shell_capsuleCollider.direction   = 2; // Z-axis
        */
        
    }

    // Start of my Functions.==============================

    private void OnCollisionEnter(Collision collision)
    {
        // Apply damage if hit a tank.
        if (collision.gameObject.TryGetComponent<Health_System>(out Health_System targetHealth))
        {
            targetHealth.TakeDamage(getDamageMultiplier);
        }

        // Create prefab for VFX + ExplosionSound.
        if (ObjectPooler.Instance != null)
        {
            GameObject explosionInstance = ObjectPooler.Instance.GetPooledExplosion();
            if (explosionInstance != null)
            {
                // Set Position and activate it
                explosionInstance.transform.position = transform.position;
                explosionInstance.transform.rotation = Quaternion.identity;
                explosionInstance.SetActive(true);

                // Play Particles
                ParticleSystem mainPS = explosionInstance.GetComponent<ParticleSystem>();
                if (mainPS != null)
                {
                    mainPS.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                    mainPS.Play(true);
                }

                // Play Explosion Sound
                AudioSource explosionAudio = explosionInstance.GetComponent<AudioSource>();
                if (explosionAudio != null)
                {
                    explosionAudio.Stop();
                    explosionAudio.Play();
                }
            }
        }

        CancelInvoke("DeactivateShell"); // Cancel the automatic deactivation.
        gameObject.SetActive(false);     // Return the shell to the pool.
    }

    private void OnEnable()
    {
        CancelInvoke("DeactivateShell");
        Invoke("DeactivateShell", lifeTime);
    }

    private void DeactivateShell()
    {
        gameObject.SetActive(false);
    }

    public void SetShooter(Collider shooter)
    {
        currentShooterCollider = shooter;
        if (currentShooterCollider != null && shell_capsuleCollider != null)
        {
            // Ignore the shooter when the shell is fired.
            Physics.IgnoreCollision(currentShooterCollider, shell_capsuleCollider, true);
        }
    }

    private void OnDisable()
    {
        if (currentShooterCollider != null && shell_capsuleCollider != null)
        {
            // Forget the shooter.
            Physics.IgnoreCollision(currentShooterCollider, shell_capsuleCollider, false); 
            currentShooterCollider = null;
        }
    }

    // End of my Functions.================================

}
