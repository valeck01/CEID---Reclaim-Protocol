using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;         // Make this script Singleton. Any other script can have easy access on that script.

    [Header("Player's Location")]
    public bool isPlayerInBase = true;

    [Header("Boss Gates Logic")]
    public int totalBossKeysInMap = 3;          // Let Inspectr assign the # of boss keys in the map.
    public bool bossGatesCanOpen = false;       // Boolean to know if all key's are collected.

    [Header("Lore Logic")]
    
    public bool[] unlockedLore = new bool[4];   // There will be only 4 lore items.

    void Awake()
    {
        Application.targetFrameRate = -1;       // Uncapped frame rate.
        QualitySettings.vSyncCount = 0;         // Disable vertical sync.

        // Initialize Singleton.
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }


    public void CheckBossKeys(int collectedKeys)
    {
        if (collectedKeys >= totalBossKeysInMap)
        {
            bossGatesCanOpen = true;
            Debug.Log("All Boss Keys gathered! Boss gates can be opened now.");
        }
    }

    public void UnlockLorePiece(int loreID)
    {
        if (loreID >= 0 && loreID < unlockedLore.Length)
        {
            unlockedLore[loreID] = true;
            Debug.Log($"Lore piece with ID {loreID} unlocked!");
        }
    }
}
