using UnityEngine;
using TMPro;

public class Health_System : MonoBehaviour
{
    [Header("Health Parameters")]
    public float Max_Health = 300f;
    public float Starting_Health = 150f;
    public float currentHealth;
    private bool  isDead;
    
    [Header("XP System")]
    public float xpReward = 30f;

    [Header("Effects")]
    public GameObject explosionPrefab;
    public GameObject bustedTankPrefab;
    public float bustedTankDestroyTime = 300f;

    [Header("Player's Death UI")]
    public GameObject inGameUIPanel;
    public GameObject deathPopupPanel;
    public TextMeshProUGUI deathPopupText;

    [Header("Death Messages")]
    public string defaultDeathMsg = "You Died At Mission! (Game Over)";
    public string timeOutDeathMsg = "You got destroyed by Nuclear Weapon! (Game Over) (Animation of a Nuclear weapon is in progress)";

    public string KilledByTankMsg = "Enemie Tank Killed you! (Game Over)";
    public string KilledByTurretMsg = "Enemie Turret Killed you! (Game Over)";
    public string KilledInBossSectorMsg = "Killed in Boss Sector. Next time prepare better! (Game Over)";

    void Start()
    {
        if (deathPopupPanel != null) deathPopupPanel.SetActive(false);

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

    public void TakeDamage(float incommingDamage,  string deathReason = "Default")
    {
        if (isDead) return; // Already dead, no further damage.

        currentHealth -= incommingDamage;                           // Reduce current health.
        Debug.Log($"{gameObject.name} got {incommingDamage} damage! Current HP: {currentHealth}");

        if (currentHealth <= 0f)
        {
            currentHealth = 0f; // Clamp health to zero.
            isDead = true;
            HandleDeath(deathReason);
            Debug.Log($"{gameObject.name} is destroyed! with reason: {deathReason}");

        }
    }

    void HandleDeath(string reason)
    {
        
        if(gameObject.CompareTag("Player"))
        {                        
            Debug.Log("Player's tank destroyed.");

            // Enable and setup death popup UI.
            if (inGameUIPanel != null) inGameUIPanel.SetActive(false);
            if (deathPopupPanel != null) deathPopupPanel.SetActive(true);
            if (deathPopupText != null)
            {
                switch (reason)
                {
                    case "TimeOutInSector3":
                        deathPopupText.text = timeOutDeathMsg;
                        break;
                    case "HitByTurret":
                        deathPopupText.text = KilledByTurretMsg;
                        break;
                    case "HitByTank":
                        deathPopupText.text = KilledByTankMsg;
                        break;
                    case "HitByBossEnemie":
                        deathPopupText.text = KilledInBossSectorMsg;
                        break;
                    default:
                        deathPopupText.text = defaultDeathMsg;
                        break;
                }
            }
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            // Pause the game.
            Time.timeScale = 0f;
        }
        else if(gameObject.CompareTag("Enemy_Player"))
        {                        
            Debug.Log("Enemy's Tank/Turret destroyed!");

            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null && player.TryGetComponent<PlayerInventory>(out PlayerInventory pInv))
            {
                pInv.AddXP(xpReward);
            }
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
            GameObject bustedTank = Instantiate(bustedTankPrefab, transform.position, transform.rotation);  // Initialize busted tank prefab
            bustedTank.transform.localScale = transform.lossyScale;                                         // Set the tank's scale
            Destroy(bustedTank, bustedTankDestroyTime);                                                     // Destroy Busted tank prefab after time that inspector assigned.
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
