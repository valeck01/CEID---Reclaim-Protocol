using UnityEngine;

public class Pickup_Bonus : MonoBehaviour
{
    [Header("Components")]
    GameObject bonusObject;
    GameObject playerObject;
    Transform myLocation;
    CapsuleCollider bonusCapsuleCollider;
    private Vector3 initialLocation;
    private Light lightPoint;

    public Transform playerLocation;

    [Header("Bonus Parameters")]
    public float rotationSpeed;
    public float upDownSpeed;
    public float repairAmount;
    public float speedBoostMultiplicator;

    [Header("Audio Effects")]
    public GameObject bonusSoundPrefab;

    public enum BonusType{ // Posibility of adding more bonus types in future.
        Health, // Healing bonus.
        Speed, // Speed boost bonus.
        None
    }
    [Header("Bonus Types")]
    public BonusType myBonusType = BonusType.None;

    // Start is called before the first frame update
    void Start()
    {
        // Initialize Components.
        bonusObject             = gameObject;
        myLocation              = transform;
        playerObject            = GameObject.FindGameObjectWithTag("Player");
        bonusCapsuleCollider    = GetComponent<CapsuleCollider>();
        
        // Get and Setup "Light" component from child object.
        lightPoint = GetComponentInChildren<Light>();
        if (lightPoint != null)
        {
            lightPoint.intensity = 50f;     // Set light intensity.
            lightPoint.range = 50f;         // Set light range.
            lightPoint.enabled = true;      // Enable the light.

            switch (myBonusType)
            {
                case BonusType.Health:
                    lightPoint.color = UnityEngine.Color.yellow;  // Set light color to yellow for health bonus.
                    break;
                case BonusType.Speed:
                    lightPoint.color = UnityEngine.Color.green;   // Set light color to green for speed bonus.                
                    break;
                case BonusType.None:
                    Debug.LogWarning("No bonus type specified so no light color set.");
                    break;
            }           
        }
        else Debug.LogWarning("No Light component found in child objects.");

        // Initialize Bonus Parameters.
        rotationSpeed           = 25f;      // degrees per second.
        upDownSpeed             = 2f;       // units per second.
        repairAmount            = 100f;     // amount of health to repair.
        speedBoostMultiplicator = 1.5f;     // speed boost multiplier.

        // Initialize Bonus Capsule Collider.
        bonusCapsuleCollider.enabled     = true;                        // Enable the collider.
        bonusCapsuleCollider.isTrigger   = true;                        // Set as trigger collider.
        bonusCapsuleCollider.center      = new Vector3(0f, 0f, 0f);     // Center of the collider.
        bonusCapsuleCollider.radius      = 0.55f;                       // Radius of the collider.
        bonusCapsuleCollider.height      = 4f;                          // Height of the collider.
        bonusCapsuleCollider.direction   = 1;                           // Y-axis

        // Store initial location.
        initialLocation = myLocation.position;

        if (playerObject != null)
        {
            playerLocation = playerObject.transform;
        }
        else
        {
            Debug.LogWarning("You must add a player object onto the scene!");
        }

        // Initialize Bonus Sound Prefab.
        bonusSoundPrefab = Resources.Load<GameObject>("AudioClips/Bonus_Colected_Prefab");
        if (bonusSoundPrefab == null)
        {
            Debug.LogWarning("Bonus sound prefab not found in Resources folder!");
            UnityEditor.EditorApplication.isPlaying = false;    // If running in the Unity Editor.
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (playerLocation != null)
        {
            transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime);  // Rotate around Y-axis.
            float newY = Mathf.Sin(Time.time * upDownSpeed);                // Calculate new Y position.
            myLocation.position = new Vector3(initialLocation.x, initialLocation.y + newY, initialLocation.z); // Update position.
        }
    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log("Bonus Triggered by " + other.gameObject.name);
        if (other.CompareTag("Player"))                     // Check if the colliding object is the player.
        {
            switch (myBonusType)
            {
                case BonusType.Health:
                    ApplyHealthBonus(other.gameObject);     // Call ApplyHealthBonus function.
                    Debug.Log("Health Bonus Applied!");
                    break;
                case BonusType.Speed:
                    ApplySpeedBonus(other.gameObject);      // Call ApplySpeedBonus function.
                    Debug.Log("Speed Bonus Applied!");
                    break;                           
            }

            // Play bonus sound effect.
            GameObject soundInstance = Instantiate(bonusSoundPrefab, transform.position, Quaternion.identity);
            Destroy(soundInstance, soundInstance.GetComponent<AudioSource>().clip.length);
            // Future: Add sound effects, visual effects, etc.-----------------------------------------------------------------------------------------------
            Destroy(bonusObject); // Destroy bonus object after applying bonus.
        }
    }

    void ApplyHealthBonus(GameObject player)
    {
        Health_System healthSystem = player.GetComponent<Health_System>();
        if (healthSystem != null)
        {
            healthSystem.Repair(repairAmount); // Repair the player by the specified amount.
        }
    }

    void ApplySpeedBonus(GameObject player)
    {
        Tank_Movement tankMovement = player.GetComponent<Tank_Movement>();
        if (tankMovement != null)
        {
            tankMovement.Max_Tank_Speed *= speedBoostMultiplicator; // Boost the player's speed.
            // Future: Implement a timer to reset speed after a duration.
        }
    }
}
