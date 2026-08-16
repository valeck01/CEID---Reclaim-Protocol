using System;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
public class AI_Behavior : MonoBehaviour
{
    [Header("Player_Components")]
    private Transform playerLocation;

    [Header("npc_Components")]
    private CapsuleCollider npc_capsuleCollider;
    private GameObject npc_GameObject;
    private Rigidbody npc_rb;
    
    [Header("NavMesh Variables")]
    private NavMeshPath path;
    private Vector3[] pathCorners = new Vector3[64];
    private int cornersCount = 0;
    private int currentCornerIndex = 0;
    private float nextPathUpdateTime = 0;
    public float pathUpdateDelayTime = 0.5f;

    [Header("Patrol/Chasing/Investigate/Shooting Settings")]
    public Vector3[] patrolLocations;
    public int currentPatrolIndex;
    public enum AIState {Patrol, Chase, Investigate}
    public AIState currentState = AIState.Patrol;
    private Vector3 lastKnownPlayerPosition;

    [Header("Tank Vehicle Parameters")]
    public float speed;                     // Max forward speed (units/sec).
    public float turnSpeed;                 // Degrees/sec rotation speed.
    public float detectionRange;            // How far the AI can see the player.
    public float detectionAngle;                // FOV angle for detection.
    public float stopAtDistanceMin;         // how close to stop from player.
    public float stopAtDistanceMax;         // How far to be stoped from player.
    public float rayheightOffset;           // height offset for raycasting to detect player.

    [Header("Tank Shooting Parameters")]
    private Transform shellSpawnPoint;       // Where the shell is spawned from.
    private float shellSize;                // Size of the fired shell.
    public float shellSpeed;                // speed of the fired shell (units/sec).
    public float fireDelayTime;             // Fire delay (seconds between shots).
    private float nextFireTime;             // Time when the AI can fire next.
    public float angleToShoot;              // Angle withing player must be to shoot.
    public float damageMultiplier;          // Damage multiplier for projectiles.

    [Range(0f, 1f)] public float velocityLerp = 0.1f; // smoothing for velocity changes

    void Start()
    {
        path = new NavMeshPath();

        // Initialize Player's Game Object.
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj == null)
        {
            Debug.LogError("Players Location by tag 'Player' not found.");
            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
            #endif
        }
        else
        {
            playerLocation = playerObj.transform;                               // Get the players transform/Location.
        }
        npc_GameObject = this.gameObject;                                       // Set unique variable for npc's gameObject.
        npc_rb = npc_GameObject.GetComponent<Rigidbody>();              
        if (npc_rb == null)                                                     // Check if npc's tank has RigidBody assigned.
        {
            Debug.LogWarning("Npc's tank does not have RigidBody:" + npc_GameObject.name);
            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;                // If running in the Unity Editor.
            #endif
            return;
        }
        npc_capsuleCollider = GetComponent<CapsuleCollider>();
        if (npc_capsuleCollider == null)                                        // Check if npc's tank has CapsuleCollider assigned.
        {
            Debug.LogWarning("Npc's tank does not have CapsuleColider:" + npc_GameObject.name);
            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;                // If running in the Unity Editor.
            #endif
            return;
        }

        // Lock the object because we move in 2D dimension.
        npc_rb.constraints =  
            RigidbodyConstraints.FreezeRotationX |                              //Lock the X rotation of the tank.
            RigidbodyConstraints.FreezeRotationZ |                              //Lock the Z rotation of the tank.
            RigidbodyConstraints.FreezePositionY;                               //Lock the Y position of the tank.

        // Lock some parameters to ensure that the object can not pass throu other objects.
        npc_rb.isKinematic = false;
        npc_rb.useGravity = true;
        npc_capsuleCollider.enabled = true;
        npc_capsuleCollider.isTrigger = false;
        npc_capsuleCollider.center = new Vector3(0.0f, 1.20f, -0.1f);
        npc_capsuleCollider.radius = 1.1f;
        npc_capsuleCollider.height = 3.15f;
        npc_capsuleCollider.direction = 2;

        // Initialize Shells Parameters.
        shellSpawnPoint = npc_GameObject.transform.Find("FirePoint");
        if (shellSpawnPoint == null)                                            // Check if shell's spawn point is found as first child.
        {
            Debug.LogError("Game Object 'FirePoint' not found as first child: " + npc_GameObject.name);
            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;                // If running in the Unity Editor.
            #endif
        }
        shellSize = 5f;                                                         // Initialize shells Size.
        

        
        // Initialize patrolling Locations.
        if (patrolLocations == null || patrolLocations.Length == 0)             // Check if Inspector assigned atleast one patrol location.
        {
            Debug.LogError("Patrol Locations are not asigned for one or more NPC's.");
            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;                // If running in the Unity Editor.
            #endif
        }
    }

    void FixedUpdate()
    {
        // Check if player is in line of sight.
        bool canSeePlayer = isPlayerInLineOfSight(playerLocation, npc_GameObject.transform);

        switch (currentState)
        {
            case AIState.Patrol:
                if (!canSeePlayer)                                              // If player is NOT in line of sight.
                {
                    patrolTerytory();                                           // Continue to patrol.
                }
                else                                                            // If player is in line of sight. Note: NPC found him.
                {
                    currentState = AIState.Chase;                               // Switch to Chase Mode.
                    cornersCount = 0;                                           // Force npc to calculate new path.
                    chasePlayer(playerLocation, npc_GameObject.transform);      // Chase the player.
                }

                break;

            case AIState.Chase:
                if (canSeePlayer)                                               // If player is in line of sight.
                {
                    chasePlayer(playerLocation, npc_GameObject.transform);      // Continue chasing the player.
                }
                else                                                            // If player is NOT in line of sight. Note: Npc lost him.
                {
                    currentState = AIState.Investigate;                         // Switch to Investigating Mode.
                    cornersCount = 0;                                           // Force npc to calculate new path.
                    investigateLastKnownPossition();                            // Investigate the last known Possition
                }

                break;

            case AIState.Investigate:
                if (!canSeePlayer)                                              // If player is NOT in line of sight.
                {
                    investigateLastKnownPossition();                            // Continue to Investigate.
                    //Note: if investigation fails, function automaticaly will change to patrol Mode.
                }
                else                                                            // If player is in line of sight. Note: NPC found him.
                {
                    currentState = AIState.Chase;                               // Switch to Chase Mode.
                    cornersCount = 0;                                           // Force npc to calculate new path.
                    chasePlayer(playerLocation, npc_GameObject.transform);      // Chase the player.
                }

                break;
        }
    }

    // Start of my Functions.==============================

    void chasePlayer(Transform player, Transform npc)
    {
        lastKnownPlayerPosition = player.position;                                                  // Always remember where npc saw the player last time.

        Vector3 playerPos2D = new Vector3(player.position.x, npc.position.y, player.position.z);    // Find the players position in x,z axes.
        float distanceToPlayer = Vector3.Distance(npc.position, playerPos2D);                       // Calculate distance between npc and player.

        if (checkIfTargetInFront(player, npc))                                                      // Try to shoot the player.
        {
            shootWhenReady();
        }

        if (distanceToPlayer <= stopAtDistanceMin && distanceToPlayer > stopAtDistanceMax)
        {
            cornersCount = 0;   // Force tank to stop moving.

            // Keep looking at player.
            Vector3 directionToPlayer = (playerPos2D - npc.position).normalized;
            if (directionToPlayer != Vector3.zero)                                      // If player not in front.
            {
                Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer); // Calculate the angle that need to rotate.
                npc_rb.MoveRotation(Quaternion.RotateTowards(npc_rb.rotation, targetRotation, turnSpeed * Time.fixedDeltaTime));
                                                                                        // Rotate towards player.
            }
        }
        else
        {
            if (Time.time >= nextPathUpdateTime)
            {
                calculatePath(player.position);
                nextPathUpdateTime = Time.time + pathUpdateDelayTime;
            }
        }

        move(speed, turnSpeed);
    }

    void patrolTerytory()
    {
        // If patrol path is not calculated.
        if (cornersCount == 0)
        {
            calculatePath(patrolLocations[currentPatrolIndex]);
        
            // Failsafe: If the targetPosition is anaccessible.
            if (cornersCount == 0) 
            {
                Debug.LogWarning("Found an anaccessible Patrol Location.");
                currentPatrolIndex++;
                if (currentPatrolIndex >= patrolLocations.Length) currentPatrolIndex = 0;
                return; 
            }
        }

        // Move towards patrol Location.
        move(speed, turnSpeed);

        // Patrol Logic.
        if (currentCornerIndex >= cornersCount)                 // Check if NPC arrived to the Patrol Location.
        {
            currentPatrolIndex++;                               // Select next Patrol Location
            if (currentPatrolIndex >= patrolLocations.Length)   // If all Patrol Locations checked.
            {
                currentPatrolIndex = 0;                         // Reset Patrol.
            }

            cornersCount = 0;                                   // Reset cornersCount to force calculation of new path.
        }
    }

    void investigateLastKnownPossition()
    {
        if (cornersCount == 0)
        {
            calculatePath(lastKnownPlayerPosition);     // Calculate Path to last known player's position.

            if (cornersCount == 0)                      // Failsafe: If the targetPosition is anaccessible.
            {
                currentState = AIState.Patrol;          // Change npc's state to PatrolMode.
                return; 
            }
        }

        move(speed, turnSpeed);                     // Move towards that position.

        if (currentCornerIndex >= cornersCount)     // If npc arived at that position. Note: he couldn't find the player.
        {            
            currentState = AIState.Patrol;          // Change npc's state to PatrolMode.
            cornersCount = 0;                       // Reset cornerCount to force npc to calculate path again in whatever mode he is.
        }
    }

    void move(float speed, float TurnSpeed)
    {   
        // If we dont have to move.
        if (cornersCount == 0 || currentCornerIndex >= cornersCount)  
        {
            Vector3 stopVelocity = new Vector3(0, npc_rb.velocity.y, 0); // Create zero velocity to stop mooving but keep y unvulnerable for proper gravity logic.
            npc_rb.velocity = Vector3.Lerp(npc_rb.velocity, stopVelocity, velocityLerp);    // Constantly change velosity to stop.
            return;                                                                         // Exit from move function.
        }

        // Logic for movement.
        Vector3 currentPos = npc_GameObject.transform.position;                 // Find Current Position of the npc
        Vector3 targetCorner = pathCorners[currentCornerIndex];                 // Find where npc have to go.
        targetCorner.y = currentPos.y;                                          // Make target corners height equal to npc's height.
        float distanceToCorner = Vector3.Distance(currentPos, targetCorner);    // Find the distance to travel till targetCorner.

        // if near the targetCorner select next targetCorner.
        if(distanceToCorner < 0.5f)
        {
            currentCornerIndex++;
            if (currentCornerIndex >= cornersCount) return;     // Stop if reached the target and dont have where to go.

            targetCorner = pathCorners[currentCornerIndex];     // Find next location of the corner.
            targetCorner.y = currentPos.y;                      // Again Make target corners height equal to npc's height.
        }

        // Turn Logic.
        Vector3 directionToCorner = (targetCorner - currentPos).normalized;
        if (directionToCorner != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(directionToCorner);
            npc_rb.MoveRotation(Quaternion.RotateTowards(npc_rb.rotation, targetRotation, TurnSpeed * Time.fixedDeltaTime));
        }

        // Move Forward Logic.
        Vector3 desiredVelocity = npc_GameObject.transform.forward * speed;                 // Calculate desired velocity for forward moving.
        desiredVelocity.y = npc_rb.velocity.y;                                              // Kepp the y velocity for proper gravity logic.
        npc_rb.velocity = Vector3.Lerp(npc_rb.velocity, desiredVelocity, velocityLerp);     // Apply desired velocity.

    }

    void calculatePath(Vector3 targetPosition)
    {
        // Calculate the path to targetPosition.
        NavMesh.CalculatePath(npc_GameObject.transform.position, targetPosition, NavMesh.AllAreas, path);

        // (Zero Allocation) update the Vector3 List with new calculated path and return the # of valid corners.
        cornersCount = path.GetCornersNonAlloc(pathCorners);
        currentCornerIndex = 0;     // Reset corner target with first calculated corner.

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
        }else                                                                           // Fire projectile.
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
                    shellBehavior.SetShooter(npc_capsuleCollider);                      // Let shellBehavior to know who is shooting.
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

    // Check if target is within 45 degree angle infront of the npc.
    bool checkIfTargetInFront(Transform player, Transform npc)
    {
        Vector3 playerPos2D = new Vector3(player.position.x, npc.position.y, player.position.z);    // Find the players position in x,z axes.
        Vector3 directionToPlayer = (playerPos2D - npc.position).normalized;
        
        float angle = Vector3.Angle(npc.forward, directionToPlayer);                                // Calculate the angle to player.
        
        return angle <= angleToShoot/2f;                                                            // Return true if withing the FOV.
    }

    bool isPlayerInLineOfSight(Transform player, Transform npc)
    {
        // Distance check.
        float distanceToPlayer = Vector3.Distance(npc.position, player.position);                   // Calculate the distance between npc and player.
        if (distanceToPlayer > detectionRange) return false;                                        // Check if the npc is close enough to see the player.

        // Angle check.
        Vector3 playerPos2D = new Vector3(player.position.x, npc.position.y, player.position.z);    // Find the players position in x,z axes.
        Vector3 directionToPlayer2D = (playerPos2D - npc.position).normalized;                      
        float angle = Vector3.Angle(npc.forward, directionToPlayer2D);                              // Calculate the angle to player.
        if (angle > detectionAngle / 2f) return false;                                              // Return False if angle is bigger than detectionAngle.

        // Distance and Angle is ok.
        // Prepare ray's variables for proper work.
        Vector3 rayOrigin = npc.position + Vector3.up * rayheightOffset;            // Set the starting point of ray.
        Vector3 targetCenter = player.position + Vector3.up * rayheightOffset;      // Find the player's gameObject center.
        Vector3 rayDirection = (targetCenter - rayOrigin).normalized;               // Set the direction of ray towards player's center.

        // Shoot rays to find if player is visible.
        if (Physics.Raycast(rayOrigin, rayDirection, out RaycastHit hit, detectionRange))
        {
            // If ray hit player's gameObject.
            if (hit.transform == player || hit.collider.CompareTag("Player"))
            {
                return true;    // Player is IN line of sight.
            }
        }

        return false;           // Player is NOT in line of sight.
    }
    // End of my Functions.==============================

}

