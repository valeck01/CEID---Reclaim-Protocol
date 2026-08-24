using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    [Header("UI & Media Settings")]
    public Image backgroundImage;                   // Default Background image.
    public Sprite customBackgroundImage;            // Let inspector change Background image.
    
    public AudioSource audioSource;
    public AudioClip backgroundMusic;               // Let inspector assign background music.

    [Header("Game Settings")]
    public string sceneToLoad = "Scene_Level_1";    // Let inspector to assign scene to load at the start.
    public GameObject exitPopupPanel;               // Get exit Button game object.

    void Start()
    {
        // Show background image.
        if (customBackgroundImage != null)
        {
            backgroundImage.sprite = customBackgroundImage;
            backgroundImage.color = Color.white;
        }
        else
        {
            Debug.LogWarning("Inspector forgot to assign background image for main menu.");
            backgroundImage.sprite = null;
            backgroundImage.color = Color.white;
        }

        // Play Background music if assigned
        if (backgroundMusic != null)
        {
            audioSource.clip = backgroundMusic;
            audioSource.Play();
        }
        else
        {
            Debug.LogWarning("Inspector forgot to assign background music for maiun menu. Nothing is on play at the moment.");
            audioSource.Stop();
        }

        // If game runns as build.exe force it to be on windowed mode.
        #if !UNITY_EDITOR
        Screen.fullScreenMode = FullScreenMode.Windowed;
        #endif

        // Exit popup panel should be inactive at the start.
        if (exitPopupPanel != null) exitPopupPanel.SetActive(false);
    }

    // "Start New Game" button function.
    public void StartNewGame()
    {
        if (!string.IsNullOrEmpty(sceneToLoad))
        {
            SceneManager.LoadScene(sceneToLoad);
        }
        else
        {
            Debug.LogError("Inspector forgot to assign the scene of the game.");
        }
    }

    // "Exit to desktop" button function.
    public void ShowExitPopup()
    {
        if (exitPopupPanel != null) exitPopupPanel.SetActive(true);     // Show warning popup.
    }

    // "No / Stay" button function.
    public void HideExitPopup()
    {
        if (exitPopupPanel != null) exitPopupPanel.SetActive(false);    // Hide warning popup.
    }

    // "Yes / Exit" button function.
    public void ConfirmExit()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
}
