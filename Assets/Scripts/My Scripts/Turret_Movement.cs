using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Turret_Movement : MonoBehaviour
{

    private CapsuleCollider tank_capsuleCollider;


    [Header("Tank Shooting Parameters")]
    public Transform shellSpawnPoint;       // Where the shell is spawned from.
    public float shellSpeed;                // speed of the fired shell (units/sec).
    public float fireDelayTime;             // Fire delay (seconds between shots).
    public float nextFireTime;              // Time when the AI can fire next.
    public float tankDamage;                // Player's shell damage.

    [Header("Audio Components")]
    [SerializeField] private AudioSource shootAudioSource;
    [SerializeField] private AudioClip shotFiringClip;
    [SerializeField] private AudioClip shotReloadClip;

    public float rotate_speed = 60f;
    // Start is called before the first frame update
    void Start()
    {
        // Initialize Audio Source
        if (shootAudioSource == null)
        {
            shootAudioSource = GetComponent<AudioSource>();
        }

        // Initialize Tank's CapsuleCollider.
        tank_capsuleCollider = GetComponentInParent<CapsuleCollider>();

        // Intialize Shooting Parameters.
        shellSpawnPoint = transform.Find("FirePoint");              // Find shell spawn point.
    }

    // Update is called once per frame
    void Update()
    {

        if (Input.GetButtonDown("Fire1") && canIshoot())                                // Check if can shoot.
        {
            GameObject projectile = ObjectPooler.Instance.GetPooledObject();            // Get an shell from the pool.
            if (projectile != null)                                                     // Check if an shell is found.
            {
                nextFireTime = Time.time + fireDelayTime;                               // Schedule shooting delay end.
                if (shootAudioSource != null && shotFiringClip != null)                 
                {
                    shootAudioSource.PlayOneShot(shotFiringClip);                       // Play firing sound.
                }
                if (shootAudioSource != null && shotReloadClip != null)
                {
                    StartCoroutine(PlayReloadSoundWithDelay(0.3f));                     // Play reload sound.
                }

                // Set correct parameters for the shell
                projectile.transform.position = shellSpawnPoint.position;               // Set shell's position.
                projectile.transform.rotation = shellSpawnPoint.rotation;               // Set shell's rotation.
                projectile.transform.localScale = shellSpawnPoint.lossyScale;           // Set shell's scale.

                if (projectile.TryGetComponent<Shell_Behavior>(out Shell_Behavior shellBehavior))
                {
                    shellBehavior.shellDamage = tankDamage;                             // Set the shell's Damage.
                    shellBehavior.SetShooter(tank_capsuleCollider);
                }
                
                projectile.SetActive(true);                                             // Activate the shell.

                if (projectile.TryGetComponent<Rigidbody>(out Rigidbody projectileRb))
                {
                    projectileRb.velocity = tank_capsuleCollider.attachedRigidbody.velocity;                // Find tanks velocity.
                    projectileRb.angularVelocity = Vector3.zero;                                            // Reset angular velocity.
                    projectileRb.AddForce(shellSpawnPoint.forward * shellSpeed, ForceMode.VelocityChange);  // Add force to shell.
                }
            }
            else
            {
                Debug.LogError("Turret_Movement.cs did not found projectile prefab.");
                #if UNITY_EDITOR
                    UnityEditor.EditorApplication.isPlaying = false;
                #endif
            }
}
    }

    void LateUpdate()
    {
        float rotate_pressed = Input.GetAxis("Horizontal2");
        transform.Rotate(Vector3.up, rotate_pressed * rotate_speed * Time.deltaTime);
    }


        // Start of my Functions.==============================
    bool canIshoot()
    {
        if (shellSpawnPoint == null) 
        {   
            Debug.LogError("Shell Spawn Point is missing! Cannot shoot.");
            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;   // If running in the Unity Editor.
            #endif
            return false;
        }

        if (Time.time < nextFireTime) //Check if ready to fire.
        {
            return false; // Not ready to fire yet.
        }
        else
        {
            return true;
        }
    }

    private IEnumerator PlayReloadSoundWithDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (shootAudioSource != null && shotReloadClip != null)
        {
            shootAudioSource.PlayOneShot(shotReloadClip);
        }
    }
    
    // End of my Functions.==============================
}


