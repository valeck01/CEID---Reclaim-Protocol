using UnityEngine;

public class Health_System : MonoBehaviour
{
    [Header("Health Parameters")]
    public float Max_Health = 300f;
    public float Starting_Health = 150f;
    public float currentHealth;
    private bool  isDead;
    
    [Header("Effects")]
    public float damageAmmount = 10f;
    public GameObject explosionPrefab;
    public GameObject bustedTankPrefab;
    public float bustedTankDestroyTime = 300f;


    
    void Start()
    {
        // Initialize explosion sound prefab.
        if (explosionPrefab == null)
        {
            Debug.LogError("Explosion Prefab is not assigned in the Inspector!");
            #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;    // If running in the Unity Editor.
            #endif
        }

        if (bustedTankPrefab == null)
        {
            Debug.LogError("Busted Tank Prefab not applyed to one or more tanks");
            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;                // If running in the Unity Editor.
            #endif
        }
        // Initialize explosion effect prefab later.
    }


    // Start of my Functions.============================
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
            Debug.Log("Enemy Tank destroyed!");
        }

        // Play explosion sound effect.
        if (explosionPrefab != null)
        {
            GameObject explosionInstance = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
            AudioSource explosionAudio = explosionInstance.GetComponent<AudioSource>();
            if (explosionAudio != null)
            {
                explosionAudio.Play();
            }

            ParticleSystem[] particles = explosionInstance.GetComponentsInChildren<ParticleSystem>();
            foreach (ParticleSystem ps in particles)
            {
                ps.Play();
            }

            // Destroy the object when the sound ends or after 3 seconds
            float destroyTime = (explosionAudio != null && explosionAudio.clip != null) ? explosionAudio.clip.length : 3f;
            Destroy(explosionInstance, destroyTime);
        }

        if (bustedTankPrefab != null)
        {
            GameObject bustedTank = Instantiate(bustedTankPrefab, transform.position, transform.rotation);
            Destroy(bustedTank, bustedTankDestroyTime);
        }
                
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
    }

    // End of my Functions.==============================
}
