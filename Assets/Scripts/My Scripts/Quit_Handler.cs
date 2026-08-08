using UnityEngine;

public class Quit_Handler : MonoBehaviour
{
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            Debug.Log("Quit command received. Exiting application.");
            
            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;    // If running in the Unity Editor.
            #else
                Application.Quit();                                 // If running as a standalone application.

                // Here could be command that pauses the game and shows a menu.
            #endif
        }
                
    }
}

