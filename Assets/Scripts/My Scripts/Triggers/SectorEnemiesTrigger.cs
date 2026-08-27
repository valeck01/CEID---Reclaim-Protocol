using UnityEngine;
using TMPro;

public class SectorEnemiesTrigger : MonoBehaviour
{
    [Header("Sector Enemies Settings")]
    public Transform enemiesParent;             // GameObject Father of npc_turret GameObjects.
    [Header("Death Timer Settings")]
    public float timeLimit = 300f;              // Τα δευτερόλεπτα που έχει ο παίκτης.
    public TextMeshProUGUI timerText;           // Timer's Text for inGame UI

    // Private variables
    private Transform[] enemyTransforms;
    private Quaternion[] enemyInitialRotations;
    private float currentTime;
    private bool isTimerActive = false;
    private Health_System playerHealth;

    void Start()
    {
        // 1. Get Player's GameObject.
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerHealth = playerObj.GetComponent<Health_System>();
        }
        else
        {
            Debug.LogError("[SectorEnemiesTrigger.cs] can't find player's tank by tag 'Player'!");
        }
        // 2. Initialize timer's UI.
        if (timerText != null)
        {
            timerText.gameObject.SetActive(false);
        }
        else
        {
            Debug.LogError("[SectorEnemiesTrigger.cs] cant find Timer's Text in inGameUI !");
        }
        // 3. Search all enemies in Sector 3 game object Parent.
        if (enemiesParent != null)
        {
            int childCount = enemiesParent.childCount;
            enemyTransforms = new Transform[childCount];
            enemyInitialRotations = new Quaternion[childCount];
            for (int i = 0; i < childCount; i++)
            {
                enemyTransforms[i] = enemiesParent.GetChild(i);
                enemyInitialRotations[i] = enemyTransforms[i].rotation; // Αποθήκευση της αρχικής κατεύθυνσης.
                
                // Κρύβουμε τα turrets στην αρχή!
                enemyTransforms[i].gameObject.SetActive(false);
            }
        }
        else
        {
            Debug.LogError("[SectorEnemiesTrigger.cs] inspector did not assign Enemies Parent Object!");
        }
    }

    void Update()
    {
        if (isTimerActive)
        {
            currentTime -= Time.deltaTime; // Calculate time that left.
            
            // Update timer's text in UI.
            if (timerText != null)
            {
                timerText.text = Mathf.Ceil(currentTime).ToString();
            }

            // When time has come.
            if (currentTime <= 0)
            {
                currentTime = 0;
                isTimerActive = false;
                if (timerText != null) timerText.text = "0";

                // Σκοτώνουμε τον παίκτη!
                if (playerHealth != null)
                {
                    Debug.Log("Time out in Sector-3! Player's Tank got 999 Damage!");
                    playerHealth.TakeDamage(999f, "TimeOutInSector3");
                    // playerHealth.TakeDamage(999f, "reason");
                }
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // 1. Start Counting the time.
            currentTime = timeLimit;
            isTimerActive = true;

            if (timerText != null)
            {
                timerText.color = Color.red;            // Make text red.
                timerText.gameObject.SetActive(true);
            }

            // 2. Activate All Enemies.
            if (enemyTransforms != null)
            {
                for (int i = 0; i < enemyTransforms.Length; i++)
                {
                    enemyTransforms[i].rotation = enemyInitialRotations[i];
                    enemyTransforms[i].gameObject.SetActive(true);          // Note: HealthSystem parameters of all enemies getting reset by default in HealthSystem.cs
                }
            }
        }
    }

    void OnTriggerExit(Collider other)      // Note: this function will be called only if player is allive and exited the sector on time.
    {
        if (other.CompareTag("Player"))
        {
            isTimerActive = false;

            // Hide timer text in UI.
            if (timerText != null)
            {
                timerText.gameObject.SetActive(false);
            }

            // Disable Enemies.
            if (enemyTransforms != null)
            {
                for (int i = 0; i < enemyTransforms.Length; i++)
                {
                    enemyTransforms[i].gameObject.SetActive(false);
                }
            }
        }
    }
}
