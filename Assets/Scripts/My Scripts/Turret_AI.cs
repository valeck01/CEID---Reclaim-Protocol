using UnityEngine;

public class Turret_AI : MonoBehaviour
{
    public enum TurretState { Searching, Aggressive }
    
    [Header("Player Reference")]
    private Transform playerLocation;

    [Header("Turret References")]
    private GameObject turret_GameObject;
    private Collider turret_Collider;               

    [Header("Turret States")]
    public TurretState currentState = TurretState.Searching;
    public bool canMoveOnSearch = false;

    [Header("Turret Movement Parameters")]
    public float turnSpeed;                         // Degrees/sec rotation speed.

    [Header("Turret Detection Parameters")]
    public float detectionRange = 10;               // How far the AI can see the player.
    public float detectionAngle = 90f;              // FOV angle for detection.
    public float rayheightOffset = 0.2f;            // Height offset for raycasting to detect player.

    [Header("Turret Shooting Parameters")]
    private Transform shellSpawnPoint;              // Where the shell is spawned from.
    public float shellSpeed;                        // Speed of the fired shell (units/sec).
    public float fireDelayTime;                     // Fire delay (seconds between shots).
    private float nextFireTime;                     // Time when the AI can fire next.
    public float tankDamage;                        // Damage for projectiles.
    public string deathReason = "HitByTurret";      // Let inspector deside player's death reason.

    [Header("Audio Effects")]
    public AudioSource shootAudioSource;
    public AudioClip shotFiringClip;

    void Start()
    {
        // Find player's GameObject.
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerLocation = playerObj.transform;
        }
        else
        {
            Debug.LogError("Player's Location by tag 'Player' not found.");
        }

        turret_GameObject = this.gameObject;
        turret_Collider = GetComponent<Collider>();

        // Initialize Shells Parameters.
        shellSpawnPoint = turret_GameObject.transform.Find("FirePoint");
        if (shellSpawnPoint == null)
        {
            Debug.LogError("[Turret_AI.cs] Game Object 'FirePoint' not found as child: " + turret_GameObject.name);
        }
        if (shootAudioSource == null || shotFiringClip == null)
        {
            Debug.LogError("[Turret_AI.cs] Audio source or clip for shooting is missing on " + turret_GameObject.name);
        }
    }

    void FixedUpdate()
    {
        // Check if player is in line of sight.
        bool canSeePlayer = isPlayerInLineOfSight(playerLocation, turret_GameObject.transform);

        // Define current state of AI Turret.
        if (canSeePlayer)
        {
            currentState = TurretState.Aggressive;
        }
        else
        {
            currentState = TurretState.Searching;
        }

        // Deside Action based on state.
        switch (currentState)
        {
            case TurretState.Searching:
                if (canMoveOnSearch){SearchForPlayer();}
                break;
            case TurretState.Aggressive:
                AttackPlayer();
                break;
        }
    }


    // Start of my Functions.============================

    void SearchForPlayer()
    {
        // Simple rotation around Y axis to search for player.
        turret_GameObject.transform.Rotate(Vector3.up * turnSpeed * Time.fixedDeltaTime);
    }

    void AttackPlayer()
    {
        // Rotate towards player.
        Vector3 playerPos2D = new Vector3(playerLocation.position.x, turret_GameObject.transform.position.y, playerLocation.position.z);
        Vector3 directionToPlayer = (playerPos2D - turret_GameObject.transform.position).normalized;
        if (directionToPlayer != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);
            turret_GameObject.transform.rotation = Quaternion.RotateTowards(turret_GameObject.transform.rotation, targetRotation, turnSpeed * Time.fixedDeltaTime);
        }

        // Calculate angle to player to ensure we are looking roughly at them before shooting.
        float angleToPlayer = Vector3.Angle(turret_GameObject.transform.forward, directionToPlayer);
        
        if (angleToPlayer < 5f) // Shoot when facing the player (within 5 degrees)
        {
            shootWhenReady();
        }
    }

    void shootWhenReady()
    {
        if (shellSpawnPoint == null) return;
        if (Time.time >= nextFireTime)                                              // Check if ready to fire.
        {
            GameObject projectile = ObjectPooler.Instance.GetPooledObject();        // Get a shell from the pool.
            if (projectile != null)
            {
                // Set correct parameters for the shell
                projectile.transform.position = shellSpawnPoint.position;
                projectile.transform.rotation = shellSpawnPoint.rotation;
                projectile.transform.localScale = shellSpawnPoint.lossyScale;

                if (projectile.TryGetComponent<Shell_Behavior>(out Shell_Behavior shellBehavior))
                {
                    shellBehavior.shellDamage = tankDamage;         // Apply shell's damage.
                    shellBehavior.damageReason = deathReason;     // Set reason in case if turret's shell will kill the player.
                    shellBehavior.SetShooter(turret_Collider);      // Prevent the turret from shooting itself
                }

                projectile.SetActive(true); // Activate the shell

                if (projectile.TryGetComponent<Rigidbody>(out Rigidbody projectileRb))
                {
                    projectileRb.velocity = Vector3.zero; // Turret is stationary, so base velocity is 0
                    projectileRb.angularVelocity = Vector3.zero;
                    projectileRb.AddForce(shellSpawnPoint.forward * shellSpeed, ForceMode.VelocityChange);
                }
            }
            nextFireTime = Time.time + fireDelayTime; // Schedule next fire time
            
            if (shootAudioSource != null && shotFiringClip != null)
            {
                shootAudioSource.PlayOneShot(shotFiringClip);
            }
        }
    }

    bool isPlayerInLineOfSight(Transform player, Transform npc)
    {
        // 1. Distance check
        float distanceToPlayer = Vector3.Distance(npc.position, player.position);
        if (distanceToPlayer > detectionRange) return false;

        // 2. Angle check (FOV)
        Vector3 playerPos2D = new Vector3(player.position.x, npc.position.y, player.position.z);
        Vector3 directionToPlayer2D = (playerPos2D - npc.position).normalized;                      
        float angle = Vector3.Angle(npc.forward, directionToPlayer2D);                              
        if (angle > detectionAngle / 2f) return false;

        // 3. Raycast check (Obstacles)
        Vector3 rayOrigin = npc.position + Vector3.up * rayheightOffset;            
        Vector3 targetCenter = player.position + Vector3.up * rayheightOffset;      
        Vector3 rayDirection = (targetCenter - rayOrigin).normalized;               
        Debug.DrawRay(rayOrigin, rayDirection * detectionRange, Color.red); // For inspector visualization
        
        if (Physics.Raycast(rayOrigin, rayDirection, out RaycastHit hit, detectionRange))
        {
            if (hit.transform == player || hit.collider.CompareTag("Player"))
            {
                return true; // Player is visible!
            }
        }
        return false;
    }


    // End of my Functions.==============================
}
