using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
public class AI_Behavior : MonoBehaviour
{
    [Header("Components")]
    private CapsuleCollider capsuleCollider;
    private GameObject playerGameObject;
    public Transform playerLocation;
    public GameObject enemyGameObject;
    private Rigidbody enemy_rb;
    public NavMeshAgent agent;
    
    [Header("Patrol/Chasing Settings")]
    private NavMeshPath path;
    public Vector3[] patrolLocations;
    public int currentPatrolIndex;
    private Vector3 currentPatrolLocation;
    private bool isPatrolling = true;

    [Header("Tank Vehicle Parameters")]
    public float chaseSpeed;            // max forward speed (units/sec).
    public float turnSpeed;             // degrees/sec rotation speed.
    public float detectionRange;        // how far the AI can see the player.
    public float fovAngle;              // field of view angle for detection.
    public float stopAtDistance;        // how close to stop from player.
    public float rayheightOffset;       // height offset for raycasting to detect player.

    [Header("Tank Shooting Parameters")]
    public GameObject shellObject;          // The shell prefab to be fired.
    public Transform shellSpawnPoint;       // Where the shell is spawned from.
    public float shellSize;                 // Volume of the fired shell
    public float shellSpeed;                // speed of the fired shell (units/sec).
    public float fireDelayTime;             // Fire delay (seconds between shots).
    public float nextFireTime;              // Time when the AI can fire next.
    public float timeToDestroyProjectile;   // Time after which the projectile is destroyed.
    public float damageMultiplier;          // Damage multiplier for projectiles.
    
    public int countexistingprojectiles;
    [Range(0f, 1f)] public float velocityLerp = 0.1f; // smoothing for velocity changes
    // Start is called before the first frame update

    void Start()
    {
        path = new NavMeshPath();
        agent = GetComponent<NavMeshAgent>();
        agent.updatePosition = false; // We will handle position updates manually.
        agent.updateRotation = false; // We will handle rotation updates manually.
        playerGameObject = GameObject.FindGameObjectWithTag("Player");
        playerLocation = playerGameObject.transform;
        enemyGameObject = this.gameObject;

        // Initialize Tank Parameters.
        chaseSpeed      = 40f;      // max forward speed (units/sec).
        turnSpeed       = 2f;       // degrees/sec rotation speed.
        detectionRange  = 150f;     // how far the AI can see the player.
        fovAngle        = 120f;     // field of view angle for detection.
        stopAtDistance  = 50f;      // how close to stop from player.
        rayheightOffset = 2f;     // height offset for raycasting to detect player.
        float staticlocationHeight = 0f;

        // Initialize AI's Rigidbody.
        enemy_rb = enemyGameObject.GetComponent<Rigidbody>();
        enemy_rb.mass           = 100f;
        enemy_rb.drag           = 5f;
        enemy_rb.angularDrag    = 5f;
        enemy_rb.isKinematic    = false;
        enemy_rb.useGravity     = true;
        enemy_rb.constraints    = (
            RigidbodyConstraints.FreezeRotationX |
            RigidbodyConstraints.FreezeRotationZ |
            RigidbodyConstraints.FreezePositionY
        );

        // Initialize AI's CapsuleCollider.
        capsuleCollider = GetComponent<CapsuleCollider>();
        capsuleCollider.enabled     = true;
        capsuleCollider.isTrigger   = false;
        capsuleCollider.center      = new Vector3(0f, 1.20f, -0.1f);
        capsuleCollider.radius      = 1.10f;
        capsuleCollider.height      = 3.15f;
        capsuleCollider.direction   = 2; // Z-axis

        // Initialize Start Location & Rotation.
        transform.position = new Vector3(155f, staticlocationHeight, -36f);
        transform.rotation = Quaternion.Euler(0f, -87f, 0f);

        // Initialize patrolling Locations.
        currentPatrolIndex = 0;
        patrolLocations = new Vector3[]
        {
            new Vector3(-80f, staticlocationHeight, 180f),
            new Vector3(33f, staticlocationHeight, 176.5f),
            new Vector3(190f, staticlocationHeight, -120f),
            new Vector3(20f, staticlocationHeight, -40f),
            new Vector3(90f, staticlocationHeight, -130f),
            new Vector3(-120f, staticlocationHeight, -10f),

        };

        if (patrolLocations.Length > 0)
        {
            currentPatrolLocation = patrolLocations[0];
        }
        else
        {
            currentPatrolLocation = transform.position; // Stay in place if no patrol points are set.
            Debug.LogWarning("No patrol points set for AI.");
        }

        // Intialize Shooting Parameters.
        shellObject     = Resources.Load<GameObject>("My_Shell");   // Load shell prefab.
        shellSpawnPoint = transform.Find("FirePoint");              // Find shell spawn point.
        shellSize       = 5f;                                       // Size of the fired shell.
        shellSpeed      = 200f;                                     // Speed of the fired shell (units/sec).
        fireDelayTime   = 3f;                                       // Fire delay (seconds between shots).
        nextFireTime    = 0f;                                       // Time when the AI can fire next.
        timeToDestroyProjectile   = 10f;                            // Time after which the projectile is destroyed.
        damageMultiplier = 1.0f;
    }

    // Start of my Functions.==============================
    bool IsPlayerInLineOfSight()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, playerLocation.position); //dont change.
        float angleBetweenFrontAndPlayer = Vector3.Angle(transform.forward, (playerLocation.position - transform.position).normalized);
        RaycastHit hit;
        Vector3 rayOrigin = transform.position + Vector3.up * rayheightOffset;
        Vector3 directionToPlayer = (playerLocation.position - transform.position).normalized;

        // Check Distance.
        if (distanceToPlayer > detectionRange)
        {
            return false;  // Out of detection Range.
        }
        
        // Check Angle.
        if (angleBetweenFrontAndPlayer > fovAngle * 0.8f)
        {
            return false; // Out of FOV angle.
        }

        // Check boundaries between AI and Player.
        if (Physics.Raycast(rayOrigin, directionToPlayer, out hit, detectionRange))
        {
            if (hit.transform == playerLocation || hit.collider.CompareTag("Player"))
            {   
                return true; // Player is in line of sight.
            }
            else return false;
        }
        else return false;

        /*        // Previus Code
        if(Vector3.Angle(transform.forward, (playerLocation.position - transform.position).normalized) <= fovAngle)
        {
            if (distanceToPlayer <= detectionRange)
            {
                RaycastHit hit;
                Vector3 rayOrigin = transform.position + Vector3.up * rayheightOffset;
                Vector3 directionToPlayer = (playerLocation.position - transform.position).normalized;
                
                if (Physics.Raycast(rayOrigin, directionToPlayer, out hit, detectionRange))
                {
                    if (hit.transform == playerLocation || hit.collider.CompareTag("Player"))
                    {   
                        //Debug.Log($"Player detected. Distance: {distanceToPlayer}");
                        return true; // Player is in line of sight.
                    }
                    else return false;
                }
                else return false;
            }
            else return false;
        }
        else return false;
        */
    }
    
    void moveToTarget(Vector3 targetPosition, float stopAtDistance, float Speed, float turnSpeed, bool patrolMode)
    {
        // Calculate the path to the target.
        NavMesh.CalculatePath(transform.position, targetPosition, NavMesh.AllAreas, path);
            
        // Always turning towards corner.
        if(path.corners.Length > 1)
        {
            Vector3 directionToNextCorner = (path.corners[1] - transform.position).normalized;
            Quaternion targetRotation = Quaternion.LookRotation(directionToNextCorner);
            enemy_rb.MoveRotation(Quaternion.RotateTowards(enemy_rb.rotation, targetRotation, turnSpeed));
        }
                        
        // Calculating distance to target.
        float distanceToTarget = Vector3.Distance(transform.position, targetPosition);
        //Debug.Log($"Distance to Target: {distanceToTarget:F2}");
            
        // Moving towards target if needed.
        if (distanceToTarget > stopAtDistance) // Start moving forward if not close enough.
        {
            Vector3 desiredVelocity = transform.forward * Speed;                // Speed = units/sec
            desiredVelocity.y = enemy_rb.velocity.y;                            // preserve vertical velocity
            enemy_rb.velocity = Vector3.Lerp(enemy_rb.velocity, desiredVelocity, velocityLerp);

                    //Debug.Log($"Enemy's velocity: {path}");
        }
        else // Stop moving when within stop distance.
        {
            Vector3 desiredVelocity = Vector3.zero;
            desiredVelocity.y = enemy_rb.velocity.y; // preserve vertical velocity
            enemy_rb.velocity = Vector3.Lerp(enemy_rb.velocity, desiredVelocity, velocityLerp);
        }

        if (patrolMode)
        {
            // Check if reached patrol point.
            if (distanceToTarget <= 1.0f) // Considered reached if within 1 unit.
            {
                // Select random patrol point.
                currentPatrolIndex = UnityEngine.Random.Range(0, patrolLocations.Length);
                currentPatrolLocation = patrolLocations[currentPatrolIndex];

                Debug.Log($"Reached patrol point. Moving to next point: {currentPatrolLocation}");
            }
        }


    }
    
    //Shoot a projectile if nextFireTime has come.
    void shootWhenReady()
    {
        if (shellObject == null || shellSpawnPoint == null)
        {
            Debug.LogError("Projectile Prefab or Spawn Point is missing! Cannot shoot.");
            return;
        }

        if (Time.time < nextFireTime) //Check if ready to fire.
        {
            return; // Not ready to fire yet.
        }
        else // Fire projectile.
        {
            GameObject projectile = Instantiate(shellObject, shellSpawnPoint.position, shellSpawnPoint.rotation);
            projectile.transform.localScale = Vector3.one * shellSize;                          // Set projectile size.
            projectile.GetComponent<Shell_Behavior>().getDamageMultiplier = damageMultiplier;   // Set damage multiplier.

            Collider projectileCollider = projectile.GetComponent<Collider>();
            Physics.IgnoreCollision(capsuleCollider, projectileCollider);                       // Ignore collision with self.

            Rigidbody projectileRb = projectile.GetComponent<Rigidbody>();
            projectileRb.AddForce(shellSpawnPoint.forward * shellSpeed, ForceMode.VelocityChange);
            
            Destroy(projectile, timeToDestroyProjectile);                                       // Destroy projectile after specified time to clean up.
            nextFireTime = Time.time + fireDelayTime;                                           // Schedule next fire time.
            
            Debug.Log("Shoot at player.");
            return;
        }

        
    }

    // Check if target is in front of enemy.
    bool checkIfTargetInFront(Transform enemyGameObject, Transform playerGameObject)
    {
        Vector3 directionToPlayer = (playerGameObject.transform.position - enemyGameObject.transform.position).normalized;
        float dotProduct = Vector3.Dot(enemyGameObject.transform.forward, directionToPlayer);   // Dot product to check if in front.
        return dotProduct > 0.90f;                                                              // True if player is in front.
        
    }

    // End of my Functions.==============================
 
    void Update()
    {     
         // Empty Update function.
    }

    void FixedUpdate()
    {
        // Check if agent and playerGameObject are still allive.
        if (agent == null || playerGameObject == null)
        {
            Debug.LogWarning("NavMeshAgent or Player GameObject not found.");
            return;
        }
        
        // Try to detect player.
        if (IsPlayerInLineOfSight()) // If player is visible.
        {   
            // Chase the player.
            isPatrolling = false;
            moveToTarget(playerLocation.position, stopAtDistance, chaseSpeed, turnSpeed, isPatrolling);

            // Shoot at the player if in front.
            if (checkIfTargetInFront(enemyGameObject.transform, playerGameObject.transform))
            {
                shootWhenReady();
            }
            
        }
        else
        {
            // Start Patroling.
            isPatrolling = true;
            moveToTarget(currentPatrolLocation, 0f, chaseSpeed, turnSpeed, isPatrolling);
        }
    
        // 
        if (countexistingprojectiles > 0)
        {
            // Here you can implement logic to track existing projectiles if needed.
            // For now, we just log the count.
            // Debug.Log($"Existing Projectiles: {countexistingprojectiles}");
        }
    }
}

