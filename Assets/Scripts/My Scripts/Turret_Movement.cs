using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Turret_Movement : MonoBehaviour
{

    private CapsuleCollider tank_capsuleCollider;


    [Header("Tank Shooting Parameters")]
    public GameObject shellObject;          // The shell prefab to be fired.
    public Transform shellSpawnPoint;       // Where the shell is spawned from.
    public float shellSize;                 // Volume of the fired shell
    public float shellSpeed;                // speed of the fired shell (units/sec).
    public float fireDelayTime;             // Fire delay (seconds between shots).
    public float nextFireTime;              // Time when the AI can fire next.
    public float timeToDestroyProjectile;   // Time after which the projectile is destroyed.
    public float damageMultiplier;          // Damage multiplier for projectiles.

    [Header("Audio Components")]
    public AudioSource shootFiringAudioSource;
    public AudioSource shootReloadAudioSource;

    public float rotate_speed = 60f;
    // Start is called before the first frame update
    void Start()
    {
        // Initialize Audio Source Array.
        AudioSource[] audioSources = GetComponents<AudioSource>();


        // Initialize Firing Audio Source.

        shootFiringAudioSource = audioSources[0];

        shootFiringAudioSource.clip = Resources.Load<AudioClip>("AudioClips/ShotFiring");   // Load Shell Firing sound.
        shootReloadAudioSource = audioSources[1];

        shootReloadAudioSource.clip = Resources.Load<AudioClip>("AudioClips/ShotReload");   // Load Shell Reload sound.

        // Check if Audio Sources are assigned correctly.
        if (shootFiringAudioSource == null || shootReloadAudioSource == null)
        {
            Debug.LogError("Audio Sources for shooting are not assigned properly!");
            UnityEditor.EditorApplication.isPlaying = false;    // If running in the Unity Editor.
        }

        // Setup Firing Audio Source.
        shootFiringAudioSource.volume = 1f;                            // Adjust volume as needed.
        shootFiringAudioSource.loop = false;                           // Do not loop the firing sound.
        shootFiringAudioSource.playOnAwake = false;                    // Do not play on awake.
        shootFiringAudioSource.spatialBlend = 1f;                      // 3D sound.
        shootFiringAudioSource.pitch = 1f;                             // Normal pitch.
        shootFiringAudioSource.rolloffMode = AudioRolloffMode.Linear;  // Linear volume rolloff.
        shootFiringAudioSource.maxDistance = 100f;                     // Max distance for sound audibility.
        shootFiringAudioSource.minDistance = 1f;                       // Min distance for sound audibility.


        // Setup Reload Audio Source.

        shootReloadAudioSource.volume = 0.5f;                           // Adjust volume as needed.
        shootReloadAudioSource.loop = false;                            // Do not loop the reload sound.
        shootReloadAudioSource.playOnAwake = false;                     // Do not play on awake.
        shootReloadAudioSource.spatialBlend = 1f;                       // 3D sound.
        shootReloadAudioSource.pitch = 1f;                              // Normal pitch.
        shootReloadAudioSource.rolloffMode = AudioRolloffMode.Linear;   // Linear volume rolloff. 

        // Initialize Tank's CapsuleCollider.
        tank_capsuleCollider = GetComponentInParent<CapsuleCollider>();

        // Intialize Shooting Parameters.
        shellObject = Resources.Load<GameObject>("My_Shell");   // Load shell prefab.
        shellSpawnPoint = transform.Find("FirePoint");              // Find shell spawn point.
        shellSize = 5f;                                       // Size of the fired shell.
        shellSpeed = 200f;                                     // Speed of the fired shell (units/sec).
        fireDelayTime = 1.5f;                                     // Fire delay (seconds between shots).
        nextFireTime = 0f;                                       // Time when the AI can fire next.
        timeToDestroyProjectile = 10f;                                      // Time after which the projectile is destroyed.
        damageMultiplier = 1.0f;                                     // Damage multiplier for projectiles.
    }

    // Start of my Functions.==============================
    bool canIshoot()
    {
        if (shellObject == null || shellSpawnPoint == null)
        {
            Debug.LogError("Projectile Prefab or Spawn Point is missing! Cannot shoot.");
            return false;
        }

        if (Time.time < nextFireTime) //Check if ready to fire.
        {
            return false; // Not ready to fire yet.
        }
        else
        {
            nextFireTime = Time.time + fireDelayTime;       // Schedule next fire time.
            shootFiringAudioSource.Play();                  // Play firing sound.
            shootReloadAudioSource.PlayDelayed(0.3f);       // Play reload sound with 1 second delay.
            return true;
        }  // Fire projectile.


    }
    // End of my Functions.==============================


    // Update is called once per frame
    void Update()
    {
        float rotate_pressed = Input.GetAxis("Horizontal2");
        transform.Rotate(Vector3.up, rotate_pressed * rotate_speed * Time.deltaTime);

        // Shoot when shoot button is pressed.
        if (Input.GetButtonDown("Fire1") && canIshoot())
        {
            // Shoot projectile.
            GameObject projectile = Instantiate(shellObject, shellSpawnPoint.position, shellSpawnPoint.rotation);
            projectile.transform.localScale = Vector3.one * shellSize;                          // Set projectile size.
            projectile.GetComponent<Shell_Behavior>().getDamageMultiplier = damageMultiplier;   // Set damage multiplier.

            Collider projectileCollider = projectile.GetComponent<Collider>();
            Physics.IgnoreCollision(tank_capsuleCollider, projectileCollider);                  // Ignore collision with self.

            Rigidbody projectileRb = projectile.GetComponent<Rigidbody>();
            projectileRb.AddForce(shellSpawnPoint.forward * shellSpeed, ForceMode.VelocityChange);
            Destroy(projectile, timeToDestroyProjectile);                                       // Destroy projectile after specified time to clean up.
        }
    }
}
