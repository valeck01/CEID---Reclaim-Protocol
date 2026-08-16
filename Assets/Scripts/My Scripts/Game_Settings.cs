using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Game_Settings : MonoBehaviour
{
    void Awake()
    {
        Application.targetFrameRate = 165;    // Set the target frame rate to 165.
        QualitySettings.vSyncCount = 0;       // Disable vertical sync.
    }
    
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            Debug.Log("Quit command received. Exiting application.");
            
            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;    // If running in the Unity Editor.
            #else
                // Here could be command that pauses the game and shows a menu.
                
                Application.Quit();                                 // If running as a standalone application.
            #endif
        }
                
    }
}
