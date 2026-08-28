using System.Collections;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    [Header("Inventory")]
    // Counters for inventory items
    public int repairItemsCount = 0;
    public int speedItemsCount = 0;
    public int reloadItemsCount = 0;
    public int bossKeysCount = 0;
    public int maxAmmountPerItem = 5;

    [Header("Buffs Amount")]
    public float repairAmount = 30f;            // Let inspector deside (Repair bonus amount).
    public float speedBuffAmount = 10f;         // Let inspector deside (Movement speed buff amount).
    public float speedBuffDuration = 5f;        // Let inspector deside (Movement speed buff duration).
    public float reloadBuffAmount = 0.5f;       // Let inspector deside (Reload delay buff amount).
    public float reloadBuffDuration = 5f;       // Let inspector deside (Reload buff duration).

    [Header("XP & Leveling System")]
    public int currentLevel = 0;                // Initialize starting level.
    public float currentXP = 0f;                // Initialize starting xp ammount.
    public float nextLevelXP = 100f;            // Let Inspector assing level1 requirements.
    public float xpIncreasePerLevel = 50f;      // Let inspector deside the strugle of the next level.
    public int availableUpgradePoints = 0;      // Initialize starting points available for upgrade.
    public int maxLevel = 10;                   // Let Inspector deside the biggest level possible.

    [Header("Stats Upgrades System")]
    public int maxUpgradesPerStat = 5;
    
    // Current upgrade counter.
    public int hpUpgradesCount = 0;
    public int damageUpgradesCount = 0;
    public int turretSpeedUpgradesCount = 0;
    
    // Let inspector deside the ammount of buff per upgrade.
    public float hpBonusPerUpgrade = 30f;
    public float damageBonusPerUpgrade = 15f;
    public float turretSpeedBonusPerUpgrade = 5f;

    [Header("Audio Clips")]
    public AudioSource pickupAudioSource;
    public AudioClip pickupAndActivationClip;   // Let inspector assign (prefub clip for collect and activate buffs).

    [Header("References")]
    // Get access to others scripts.
    private Health_System healthSystem;
    private New_Tank_Movement tankMovement;
    private Turret_Movement turretMovement;

    // Booleans to deside when player can activate a buff
    private bool isSpeedBuffActive = false;
    private bool isReloadBuffActive = false;

    void Start()
    {
        // Check if all prefabes are assigned.
        if (pickupAudioSource == null) 
        {
            Debug.LogError($"GameObject {gameObject.name} does not have Pickup Audio Source in [PlayerInventory.cs] component!");
            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;                // If running in the Unity Editor.
            #endif
        }
        
        if (pickupAndActivationClip  == null) 
        {
            Debug.LogError($"In GameObject {gameObject.name} Pickup Clip Prefab is not assigned in [PlayerInventory.cs] component!");
            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;                // If running in the Unity Editor.
            #endif
        }

        // Get access to player's scripts.
        healthSystem = GetComponent<Health_System>();                   // To call repair function.
        tankMovement = GetComponent<New_Tank_Movement>();               // To change tank's speed.
        turretMovement = GetComponentInChildren<Turret_Movement>();     // To change reloadDelay time.

        // Check if all scripts are assigned in player's tank.
        if (healthSystem == null)
        {
            Debug.LogError($"GameObject {gameObject.name} does not have [Health_System.cs] component.");
            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;                // If running in the Unity Editor.
            #endif
        }
        if (tankMovement == null)
        {
            Debug.LogError($"GameObject {gameObject.name} does not have [Tank_Movement.cs] component.");
            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;                // If running in the Unity Editor.
            #endif
        }
        if (turretMovement == null)
        {
            Debug.LogError($"GameObject {gameObject.name} does not have [Turret_Movement.cs] component.");
            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;                // If running in the Unity Editor.
            #endif
        }
    }

    void Update()
    {
        // Numpad 1 : Repair.
        if (Input.GetKeyDown(KeyCode.Keypad1))                              // If Numpad1 button is pressed.
        {
            if (repairItemsCount > 0)                                       // and if player has atleast one Repair item.
            {
                repairItemsCount--;                                         // Reduse repair item by one in inventory.
                healthSystem.Repair(repairAmount);                          // Apply repair on player's tank.
                PlayBuffSound();                                            // Play "Apply" sound efect.
                Debug.Log($"Repair Applyed! Remain: {repairItemsCount}");    
            }
        }

        // Numpad 2 : Movement Speed Buff.
        if (Input.GetKeyDown(KeyCode.Keypad2))                                  // If Numpad2 button is pressed.
        {
            if (speedItemsCount > 0 && !isSpeedBuffActive)                      // and if player has atleast one Speed buff item.
            {
                speedItemsCount--;                                              // Reduse speed buff item by one in inventory.
                StartCoroutine(SpeedBuffCoroutine());                           // Apply speed buff on player's tank.
                PlayBuffSound();                                                // Play "Apply" sound efect.
                Debug.Log($"Speed Buff Applyed! Remain: {speedItemsCount}");
            }
            else if (isSpeedBuffActive)
            {
                Debug.Log("Player must wait before Apply Movement Speed buff again.");
            }
        }

        // Numpad 3 : Reload Speed Buff.
        if (Input.GetKeyDown(KeyCode.Keypad3))                              // If Numpad3 button is pressed.
        {
            if(reloadItemsCount > 0 && !isReloadBuffActive)                 // and if player has atleast one Reload buff item.
            {
                reloadItemsCount--;                                         // Reduse reload speed buff item by one in inventory.
                StartCoroutine(ReloadBuffCoroutine());                      // Apply reload speed buff on player's turret.
                PlayBuffSound();                                            // Play "Apply" sound efect.
                Debug.Log($"Reload Buff Applyed! Remain:: {reloadItemsCount}");
            }
            else if (isReloadBuffActive)
            {
                Debug.Log("Player must wait before Apply Reload Speed buff again.");
            }
        }

    }


    // Start of my functions---------------------------

    // Function to play Buff Sound.
    private void PlayBuffSound()
    {
        if (pickupAudioSource != null && pickupAndActivationClip != null)
        {
            pickupAudioSource.PlayOneShot(pickupAndActivationClip);
        }
    }

    // Function to apply Movement Speed Buff.
    private IEnumerator SpeedBuffCoroutine()
    {
        isSpeedBuffActive = true;                                   // Do not let player to activate same buff.
        tankMovement.Max_Tank_Speed += speedBuffAmount;             // Apply Movement Speed Buff.
        
        yield return new WaitForSeconds(speedBuffDuration);         // Wait before disapply players buff.
        
        tankMovement.Max_Tank_Speed -= speedBuffAmount;             // Disapply players buff.
        isSpeedBuffActive = false;                                  // Let player to activate same buff again.
    }

    // Function to apply Reload Speed Buff.
    private IEnumerator ReloadBuffCoroutine()
    {
        isReloadBuffActive = true;                                          // Do not let player to activate same buff.
        
        float actualBuff = reloadBuffAmount;                        
        if (turretMovement.fireDelayTime - reloadBuffAmount < 0.1f)         // If reload buff ammount is bigget than default reload delay.
        {
            actualBuff = turretMovement.fireDelayTime - 0.1f;               // Calculate biggest safe buff.
        }
        turretMovement.fireDelayTime -= actualBuff;                         // Reduse reload delay.
        
        yield return new WaitForSeconds(reloadBuffDuration);                // Wait before disapply players buff.
        
        turretMovement.fireDelayTime += actualBuff;                         // Disapply players buff.
        isReloadBuffActive = false;                                         // Let player to activate same buff again.
    }

    // Function to let Pickables.cs to ++ Inventory's counters
    public void AddItem(string itemType, int loreID = 0)
    {

        PlayBuffSound();                        // Play sound when an item is picked-up.

        switch (itemType)
        {
            case "RepairType":
                if(repairItemsCount < maxAmmountPerItem) repairItemsCount++;
                break;

            case "MovementSpeedType":
                if(speedItemsCount < maxAmmountPerItem) speedItemsCount++;
                break;

            case "ReloadBuffType":
                if(reloadItemsCount < maxAmmountPerItem) reloadItemsCount++;
                break;

            case "LoreItemType":
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.UnlockLorePiece(loreID); // Send Lore Id to gameManager.cs
                }
                break;
            
            case "BossKeyItemType":
                bossKeysCount++;
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.CheckBossKeys(bossKeysCount);  // Ask GameManager if gates can be opened now.
                }
                break;

            // If givven Item Type is not recognizable.
            default:
                Debug.LogWarning($"[PlayerInventory] Item type is not recognized: {itemType}");
                #if UNITY_EDITOR
                    UnityEditor.EditorApplication.isPlaying = false;                // If running in the Unity Editor.
                #endif
                break;
        }


    }

    public void AddXP(float amount)
    {
        if (currentLevel >= maxLevel) return;                                   // Ignore if max level reached.
        currentXP += amount;                                                    // Increase XP ammount.
        Debug.Log($"player got {amount} XP. Total: {currentXP} / {nextLevelXP}");
 
        while (currentXP >= nextLevelXP && currentLevel < maxLevel)             // In case player got too many XP's
        {
            LevelUp();                                                          // Apply level up on player.
        }
    }

    private void LevelUp()      // This function will be not called if max level achieved.
    {
        currentLevel++;
        availableUpgradePoints++;

        // if player got more xp that needed to levelUp then keep the remain xp's for next levelUp.
        currentXP -= nextLevelXP;

        // Make next level to be harder to achieve.
        nextLevelXP += xpIncreasePerLevel; 
        Debug.Log($"Level Up! New Level is: {currentLevel}. Current xp's: {availableUpgradePoints}");
    }

    public void UpgradeMaxHP()
    {
        if (availableUpgradePoints > 0 && hpUpgradesCount < maxUpgradesPerStat)
        {
            availableUpgradePoints--;
            hpUpgradesCount++;
            healthSystem.Max_Health += hpBonusPerUpgrade;
            healthSystem.currentHealth += hpBonusPerUpgrade;
            Debug.Log($"Upgrade Max HP! (+{hpBonusPerUpgrade})");
        }
    }

    public void DowngradeMaxHP()
    {
        if (hpUpgradesCount > 0)
        {
            availableUpgradePoints++;
            hpUpgradesCount--;
            healthSystem.Max_Health -= hpBonusPerUpgrade;
            
            if (healthSystem.currentHealth > healthSystem.Max_Health)
            {
                healthSystem.currentHealth = healthSystem.Max_Health;
            }
            Debug.Log("Downgrade Max HP!");
        }
    }

    public void UpgradeDamage()
    {
        if (availableUpgradePoints > 0 && damageUpgradesCount < maxUpgradesPerStat)
        {
            availableUpgradePoints--;
            damageUpgradesCount++;
            turretMovement.tankDamage += damageBonusPerUpgrade;
            Debug.Log($"Upgrade Shell's Damage! (+{damageBonusPerUpgrade})");
        }
    }
    public void DowngradeDamage()
    {
        if (damageUpgradesCount > 0)
        {
            availableUpgradePoints++;
            damageUpgradesCount--;
            turretMovement.tankDamage -= damageBonusPerUpgrade;
            Debug.Log("Downgrade Shell's Damage!");
        }
    }

    public void UpgradeTurretSpeed()
    {
        if (availableUpgradePoints > 0 && turretSpeedUpgradesCount < maxUpgradesPerStat)
        {
            availableUpgradePoints--;
            turretSpeedUpgradesCount++;
            turretMovement.rotate_speed += turretSpeedBonusPerUpgrade;
            Debug.Log($"Upgrade Turret's movement Speed! (+{turretSpeedBonusPerUpgrade})");
        }
    }
    public void DowngradeTurretSpeed()
    {
        if (turretSpeedUpgradesCount > 0)
        {
            availableUpgradePoints++;
            turretSpeedUpgradesCount--;
            turretMovement.rotate_speed -= turretSpeedBonusPerUpgrade;
            Debug.Log("Downgrade Turret's movement Speed!");
        }
    }

    // End of my functions-----------------------------
}
