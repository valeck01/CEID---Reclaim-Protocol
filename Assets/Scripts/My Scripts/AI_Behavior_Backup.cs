using System;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
public class AI_Behavior_Backup : MonoBehaviour
{
    [Header("Components")]
    private CapsuleCollider capsuleCollider;
    private GameObject playerGameObject;
    public Transform playerLocation;
    public GameObject enemyGameObject;
    private Rigidbody enemy_rb;
    public NavMeshAgent agent;
    private float nextPathUpdateTime = 0f;
    public float pathUpdateDelay; // Ανανέωση του μονοπατιού κάθε 0.2 δευτερόλεπτα

    [Header("Patrol/Chasing Settings")]
    private NavMeshPath path;
    private Vector3[] pathCorners = new Vector3[64];
    private int cornersCount = 0;
    public Vector3[] patrolLocations;
    public int currentPatrolIndex;
    private Vector3 currentPatrolLocation;
    private bool isPatrolling = true;

    [Header("Tank Vehicle Parameters")]
    public float chaseSpeed;                // max forward speed (units/sec).
    public float turnSpeed;                 // degrees/sec rotation speed.
    public float detectionRange;            // how far the AI can see the player.
    public float fovAngle;                  // field of view angle for detection.
    public float stopAtDistance;            // how close to stop from player.
    public float rayheightOffset;           // height offset for raycasting to detect player.

    [Header("Tank Shooting Parameters")]
    public Transform shellSpawnPoint;       // Where the shell is spawned from.
    private float shellSize;                 // Volume of the fired shell
    public float shellSpeed;                // speed of the fired shell (units/sec).
    public float fireDelayTime;             // Fire delay (seconds between shots).
    private float nextFireTime;              // Time when the AI can fire next.
    public float damageMultiplier;          // Damage multiplier for projectiles.

    [Range(0f, 1f)] public float velocityLerp = 0.1f; // smoothing for velocity changes
    // Start is called before the first frame update

    void Start()
    {
        shellSize = 5f;

        path = new NavMeshPath();
        agent = GetComponent<NavMeshAgent>();
        agent.updatePosition = false;   // We will handle position updates manually.
        agent.updateRotation = false;   // We will handle rotation updates manually.
        playerGameObject = GameObject.FindGameObjectWithTag("Player");
        playerLocation = playerGameObject.transform;
        enemyGameObject = this.gameObject;

        enemy_rb = enemyGameObject.GetComponent<Rigidbody>();               // Initialize AI's Rigidbody.
        capsuleCollider = GetComponent<CapsuleCollider>();                  // Initialize AI's CapsuleCollider.

        // Lock the object because we move in 2D dimension.
        enemy_rb.constraints = RigidbodyConstraints.FreezeRotationX |     //Lock the X rotation of the tank.
                         RigidbodyConstraints.FreezeRotationZ |     //Lock the Z rotation of the tank.
                         RigidbodyConstraints.FreezePositionY;      //Lock the Y position of the tank.

        // Lock some parameters to ensure that the object can not pass throu other objects.
        enemy_rb.isKinematic = false;
        enemy_rb.useGravity = false;
        capsuleCollider.enabled = true;
        capsuleCollider.isTrigger = false;
        capsuleCollider.center = new Vector3(0.0f, 1.20f, -0.1f);
        capsuleCollider.radius = 1.1f;
        capsuleCollider.height = 3.15f;
        capsuleCollider.direction = 2;
        
        // Intialize Shooting Parameters.
        shellSpawnPoint = transform.Find("FirePoint");                      // Find shell spawn point.
        
        // Initialize patrolling Locations.
        currentPatrolIndex = 0;

        if (patrolLocations.Length > 0)
        {
            currentPatrolLocation = patrolLocations[0];
        }
        else
        {
            currentPatrolLocation = transform.position;         // Stay in place if no patrol points are set.
            Debug.LogWarning("No patrol points set for AI.");
        }
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
        if (angleBetweenFrontAndPlayer > fovAngle / 2f)
        {
            return false; // Out of FOV angle.
        }

        // Check boundaries between AI and Player.
        if (Physics.Raycast(rayOrigin, directionToPlayer, out hit, detectionRange))
        {
            if (hit.transform == playerLocation || hit.collider.CompareTag("Player"))
            {
                //Debug.Log($"Player is in line of sight.");
                return true; // Player is in line of sight.
            }
            else return false;
        }
        else return false;
    }

    void moveToTarget(Vector3 targetPosition, float stopAtDistance, float Speed, float turnSpeed, bool patrolMode)
    {
        // Calculating distance to target.
        float distanceToTarget = Vector3.Distance(transform.position, targetPosition);
        
        // Calculate the path to the target.
        if (Time.time >= nextPathUpdateTime)
        {
            NavMesh.CalculatePath(transform.position, targetPosition, NavMesh.AllAreas, path);
            cornersCount = path.GetCornersNonAlloc(pathCorners);
            nextPathUpdateTime = Time.time + pathUpdateDelay;
        }

        // Always turning towards corner.
        if (!patrolMode && distanceToTarget <= stopAtDistance) // Only turn to face the player if not in patrol mode and within stopAtdistance.
        {
            Vector3 directionToTarget = (targetPosition - transform.position).normalized;
            Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
            enemy_rb.MoveRotation(Quaternion.RotateTowards(enemy_rb.rotation, targetRotation, turnSpeed * Time.fixedDeltaTime));
        }
        
        else if (cornersCount > 1)                            //
        {
            Vector3 directionToNextCorner = (pathCorners[1] - transform.position).normalized;
            Quaternion targetRotation = Quaternion.LookRotation(directionToNextCorner);
            enemy_rb.MoveRotation(Quaternion.RotateTowards(enemy_rb.rotation, targetRotation, turnSpeed * Time.fixedDeltaTime));
        }

        
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
                // Select next patrol point.
                currentPatrolIndex = (currentPatrolIndex + 1) % patrolLocations.Length;
                currentPatrolLocation = patrolLocations[currentPatrolIndex];

                Debug.Log($"Reached patrol point. Moving to next point: {currentPatrolLocation}");
            }
        }
    }

    //Shoot a projectile if nextFireTime has come.
    void shootWhenReady()
    {
        if (shellSpawnPoint == null)
        {
            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;    // If running in the Unity Editor.
            #endif
            Debug.LogError("Spawn Point (FirePoint) is missing! Cannot shoot.");
            return;
        }

        if (Time.time < nextFireTime) //Check if ready to fire.
        {
            return; // Not ready to fire yet.
        }
        else                         // Fire projectile.
        {
            GameObject projectile = ObjectPooler.Instance.GetPooledObject();            // Get an shell from the pool.
            if (projectile != null)                                                     // Check if an shell is found.
            {
                // Set correct parameters for the shell
                projectile.transform.position = shellSpawnPoint.position;               // Set shell's position.
                projectile.transform.rotation = shellSpawnPoint.rotation;               // Set shell's rotation.
                projectile.transform.localScale = Vector3.one * shellSize;              // Set shell's scale.
                projectile.SetActive(true);                                             // Activate the shell.

                if (projectile.TryGetComponent<Shell_Behavior>(out Shell_Behavior shellBehavior))
                {
                    shellBehavior.getDamageMultiplier = damageMultiplier;               // Set the damage multiplier.
                    shellBehavior.SetShooter(capsuleCollider);
                }

                if (projectile.TryGetComponent<Rigidbody>(out Rigidbody projectileRb))
                {
                    projectileRb.velocity = Vector3.zero;                                                   // Reset velocity.
                    projectileRb.angularVelocity = Vector3.zero;                                            // Reset angular velocity.
                    projectileRb.AddForce(shellSpawnPoint.forward * shellSpeed, ForceMode.VelocityChange);  // Add force to shell.
                }
            }
            nextFireTime = Time.time + fireDelayTime;                                   // Schedule next fire time.
            Debug.Log("Enemy has shot a projectile.");
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
    }
}

