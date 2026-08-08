using UnityEngine;

public class Health_System : MonoBehaviour
{
    [Header("Health Parameters")]
    public float Max_Health = 300f;
    public float Starting_Health = 150f;
    public float currentHealth;
    public bool  isDead;
    
    [Header("Effects")]
    public float damageAmmount = 10f;
    public GameObject explosionPrefab;

    private GameObject explosionSoundPrefab;

    // Future: Add explosion effects, damage indicators, etc.-----------------------------------------------------------------------------------------------------

    void OnEnable()
    {
        //Reset health and status when enabled.
        currentHealth = Starting_Health;
        isDead = false;    
    }

    public void TakeDamage(float damageMultiplier)
    {
        if (isDead) return; // Already dead, no further damage.

        float damageDealt = damageAmmount * damageMultiplier;   // Calculate damage dealt.

        currentHealth -= damageDealt;                           // Reduce current health.
        Debug.Log($"{gameObject.name} took {damageDealt} damage! Current HP: {currentHealth}");
        // Here comes Ui for health bar update in future.---------------------------------------------------------------------------------------------------------

        if (currentHealth <= 0f && !isDead)
        {
            currentHealth = 0f; // Clamp health to zero.
            isDead = true;
            HandleDeath();
            Debug.Log($"{gameObject.name} is destroyed!");

            // Here comes explosion effect instantiation in future.------------------------------------------------------------------------------------------------
        }
    }

    void HandleDeath()
    {
        
        if(gameObject.CompareTag("Player"))
        {                        
            Debug.Log("You died at war! Game Over.");
        }
        else if(gameObject.CompareTag("Enemy_Player"))
        {                        
            Debug.Log("Enemy Tank destroyed! You win this round.");
            // Future: Here comes code that restarts the game if user wants to play again.----------------------------------------------------------------
        }

        // Play explosion sound effect.
        GameObject soundInstance = Instantiate(explosionSoundPrefab, transform.position, Quaternion.identity);
        Destroy(soundInstance, soundInstance.GetComponent<AudioSource>().clip.length);  // Destroy after sound finishes.

        // Future: explosion effects ----------------------------------------------------------------------------------------------------------
                
        gameObject.SetActive(false); // Deactivate the object.
    }
    
    public void Repair(float repairAmount)
    {
        if (isDead) return; // Cannot repair a dead object.

        currentHealth += repairAmount;
        if (currentHealth > Max_Health)
        {
            currentHealth = Max_Health; // Clamp health to max.
        }
        Debug.Log($"{gameObject.name} repaired by {repairAmount}. Current HP: {currentHealth}");
        // Here comes Ui for health bar update in future.---------------------------------------------------------------------------------------------------------
    }
    void Start()
    {
        // Initialize explosion sound prefab.
        explosionSoundPrefab = Resources.Load<GameObject>("AudioClips/Tank_Explosion_Sound_Prefab");
        if (explosionSoundPrefab == null)
        {
            Debug.LogError("ExplosionAudio Prefab not found in Resources folder! Sound cannot be played.");
            UnityEditor.EditorApplication.isPlaying = false;    // If running in the Unity Editor.
        }
        // Initialize explosion effect prefab later.
    }

    void Update()
    {
        
    }
}
