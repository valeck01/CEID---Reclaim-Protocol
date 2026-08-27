using UnityEngine;
using UnityEngine.UI;
using TMPro;                            // For TextMeshPro
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    [Header("Player References")]
    public GameObject player;
    private Health_System playerHealth;
    private New_Tank_Movement playerMovement;
    private Turret_Movement playerTurret;
    private PlayerInventory playerInventory;
    private Rigidbody playerRb;

    [Header("In-Game UI: Top Left (Stats)")]
    public TextMeshProUGUI hpText;
    public TextMeshProUGUI speedText;
    public TextMeshProUGUI damageText;
    public TextMeshProUGUI turretSpeedText;

    [Header("In-Game UI: Top Right (Level)")]
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI xpText;
    public TextMeshProUGUI maxXpText;

    [Header("In-Game UI: Bottom Center (Items)")]
    public TextMeshProUGUI repairCountText;
    public TextMeshProUGUI speedBuffCountText;
    public TextMeshProUGUI reloadBuffCountText;
    public TextMeshProUGUI bossKeysCountText;

    [Header("Pause UI Panels")]
    public GameObject inGameUIPanel;
    public GameObject pauseUIPanel;
    public GameObject mainPauseMenuPanel;
    public GameObject tunePagePanel;
    public GameObject lorePagePanel;
    public GameObject exitConfirmationPopup;

    [Header("Victory UI")]
    public GameObject victoryPopupPanel;

    [Header("Tune Page UI")]
    public TextMeshProUGUI availablePointsText;
    public TextMeshProUGUI hpUpgradeLevelText;
    public TextMeshProUGUI damageUpgradeLevelText;
    public TextMeshProUGUI turretSpeedUpgradeLevelText;

    [Header("Base & Tune Page")]
    public Button tunePageButton;
    public TextMeshProUGUI tunePageButtonText;

    [Header("Lore Page UI")]
    public Button[] loreButtons;                
    public TextMeshProUGUI[] loreButtonTexts;
    public GameObject loreTextPopup;
    public TextMeshProUGUI lorePopupText;
    [TextArea(3, 10)]
    public string[] loreTexts;                  // Let Inspector to write lore text.
    
    private bool isPaused = false;

    void Start()
    {
        if (victoryPopupPanel != null) victoryPopupPanel.SetActive(false);
        if (exitConfirmationPopup != null) exitConfirmationPopup.SetActive(false);

        if (player == null) player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerHealth = player.GetComponent<Health_System>();
            playerMovement = player.GetComponent<New_Tank_Movement>();
            playerTurret = player.GetComponentInChildren<Turret_Movement>();
            playerInventory = player.GetComponent<PlayerInventory>();
            playerRb = player.GetComponent<Rigidbody>();
        }
        ResumeGame();
    }

    void Update()
    {
        // ESC = Pause / Unpause
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            //PauseGame();
            
            if (isPaused) ResumeGame();
            else PauseGame();
            
        }
        
        UpdateInGameUI();
    }

    private void UpdateInGameUI()
    {
        if (player == null) return;

        // Top Left stats.
        if (hpText != null) hpText.text = $"HP: {playerHealth.currentHealth} / {playerHealth.Max_Health}";
        if (speedText != null) speedText.text = $"Speed: {playerRb.velocity.magnitude:F1}"; // To :F1 δείχνει 1 δεκαδικό πχ 5.2
        if (damageText != null) damageText.text = $"Damage: {playerTurret.tankDamage}";
        if (turretSpeedText != null) turretSpeedText.text = $"Turret Speed: {playerTurret.rotate_speed}";

        // Top right Stats.
        if (levelText != null) levelText.text = $"Level: {playerInventory.currentLevel}";
        if (xpText != null) xpText.text = $"XP: {playerInventory.currentXP}";
        if (maxXpText != null) maxXpText.text = $"Next Level at: {playerInventory.nextLevelXP} XP";

        // Bottom center (pickables).
        if (repairCountText != null) repairCountText.text = playerInventory.repairItemsCount.ToString();
        if (speedBuffCountText != null) speedBuffCountText.text = playerInventory.speedItemsCount.ToString();
        if (reloadBuffCountText != null) reloadBuffCountText.text = playerInventory.reloadItemsCount.ToString();
        if (bossKeysCountText != null) bossKeysCountText.text = $"{playerInventory.bossKeysCount} / {GameManager.Instance.totalBossKeysInMap}";
    }

    public void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f;                // Pause's the game.
        inGameUIPanel.SetActive(false);
        pauseUIPanel.SetActive(true);
        OpenMainPauseMenu();
    }

    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;                // Unpause's the game.
        inGameUIPanel.SetActive(true);
        pauseUIPanel.SetActive(false);
    }

    public void OpenMainPauseMenu()
    {
        mainPauseMenuPanel.SetActive(true);
        tunePagePanel.SetActive(false);
        lorePagePanel.SetActive(false);
        exitConfirmationPopup.SetActive(false);

        if (tunePageButton != null && tunePageButtonText != null)
        {
            if (GameManager.Instance.isPlayerInBase)                // Ask GameManager.cs if player is in base.
            {
                tunePageButton.interactable = true;
                tunePageButtonText.text = "Tune Page"; 
            }
            else
            {
                tunePageButton.interactable = false;
                tunePageButtonText.text = "Go to Base to Tune!";
            }
        }
    }

    public void ExitToMainMenuBtn()
    {
        exitConfirmationPopup.SetActive(true);
    }

    public void ConfirmExitYes()
    {
        Time.timeScale = 1f;                    // Game has to be unpaused in order to load another scene.
        SceneManager.LoadScene("Main_Menu"); 
    }
    public void ConfirmExitNo()
    {
        exitConfirmationPopup.SetActive(false);
    }

    public void DeathPopup_MainMenu_Btn()
    {
        Time.timeScale = 1f;                    // Note: if time is zero then MainMenu will not work.
        SceneManager.LoadScene("Main_Menu"); 
    }
    public void DeathPopup_QuitGame_Btn()
    {
        Debug.Log("Κλείσιμο Παιχνιδιού!");
        Application.Quit();
    }

    public void OpenTunePage()
    {
        mainPauseMenuPanel.SetActive(false);
        tunePagePanel.SetActive(true);
        UpdateTunePageUI();
    }

    public void UpdateTunePageUI()
    {
        if (availablePointsText != null) availablePointsText.text = $"Available Points: {playerInventory.availableUpgradePoints}";
        if (hpUpgradeLevelText != null) hpUpgradeLevelText.text = playerInventory.hpUpgradesCount.ToString();
        if (damageUpgradeLevelText != null) damageUpgradeLevelText.text = playerInventory.damageUpgradesCount.ToString();
        if (turretSpeedUpgradeLevelText != null) turretSpeedUpgradeLevelText.text = playerInventory.turretSpeedUpgradesCount.ToString();
    }

    // Functions to (+) and (-) tunes in Tune Page.
    public void Btn_UpgradeHP() { playerInventory.UpgradeMaxHP(); UpdateTunePageUI(); }
    public void Btn_DowngradeHP() { playerInventory.DowngradeMaxHP(); UpdateTunePageUI(); }
    public void Btn_UpgradeDamage() { playerInventory.UpgradeDamage(); UpdateTunePageUI(); }
    public void Btn_DowngradeDamage() { playerInventory.DowngradeDamage(); UpdateTunePageUI(); }
    public void Btn_UpgradeTurret() { playerInventory.UpgradeTurretSpeed(); UpdateTunePageUI(); }
    public void Btn_DowngradeTurret() { playerInventory.DowngradeTurretSpeed(); UpdateTunePageUI(); }

    // open page with lores
    public void OpenLorePage()
    {
        mainPauseMenuPanel.SetActive(false);
        lorePagePanel.SetActive(true);
        loreTextPopup.SetActive(false);

        // Check which lore items are collected and can be shown.
        for (int i = 0; i < loreButtons.Length; i++)
        {
            if (i < GameManager.Instance.unlockedLore.Length && GameManager.Instance.unlockedLore[i] == true)
            {
                loreButtons[i].interactable = true;
                loreButtonTexts[i].text = $"Read #{i + 1} Lore";
            }
            else
            {
                loreButtons[i].interactable = false;
                loreButtonTexts[i].text = "Lore page not collected yet";
            }
        }
    }

    // Open popup to read the lore.
    public void ReadLore(int loreIndex) 
    {
        if (loreIndex >= 0 && loreIndex < loreTexts.Length)
        {
            lorePopupText.text = loreTexts[loreIndex];
            loreTextPopup.SetActive(true);
        }
    }

    public void CloseLorePopup()
    {
        loreTextPopup.SetActive(false);
    }

    public void ShowVictoryPopup()
    {
        if (inGameUIPanel != null) inGameUIPanel.SetActive(false);
        if (victoryPopupPanel != null) victoryPopupPanel.SetActive(true);
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        Time.timeScale = 0f;
    }
}
