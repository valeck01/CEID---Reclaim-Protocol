using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Camera_Flow))]
public class Camera_Flow_Editor : Editor
{
    private List<string> targetTags = new List<string> { "Player_Turret", "Enemy_Player_Turret" }; // Type here all tags you want to follow.


    private int selectedIndex = 0;

    public override void OnInspectorGUI()
    {
        // Keep Default Parameters of the "Camera" object and its Inspector.
        DrawDefaultInspector();                                                         // Show the default Inspector of the object "Camera".
        Camera_Flow script = (Camera_Flow)target;                                       // Keep the same script that Camera_Flow.cs has.
        EditorGUILayout.Space();                                                        // Add some space between the default Inspector and the customed Inspector.
        EditorGUILayout.LabelField("Select Tank to Follow", EditorStyles.boldLabel);    // Show the dropdown to select target to follow.

        // Find the object "All Tanks" or type error.
        GameObject parentObj = GameObject.Find("All Tanks");

        if (parentObj == null)
        {
            EditorGUILayout.HelpBox("Not Found 'All Tanks' object in scene!", MessageType.Error);
            return;
        }

        // Search and fill the list with childrens that has the target tags.
        List<GameObject> foundedObjectsWithSelectedTags = new List<GameObject>();      // Create an empty list.



        Transform[] allChildren = parentObj.GetComponentsInChildren<Transform>(true);  // Get all children of the object "All Tanks".

        // Check for the tag of the child.
        foreach (Transform child in allChildren)
        {
            if (targetTags.Contains(child.tag))
            {
                foundedObjectsWithSelectedTags.Add(child.gameObject);                  // If true then save that child object in the list.
            }
        }

        if (foundedObjectsWithSelectedTags.Count == 0)                                 // If no objects are found with the target tags.
        {
            string tagsString = string.Join(", ", targetTags);
            EditorGUILayout.HelpBox($"The 'All Tanks' object does not have any objects with the following tags: {tagsString}", MessageType.Warning);
            return;
        }

        // Create, Fill and Show the dropdown menu for founded objects to follow.
        string[] options = new string[foundedObjectsWithSelectedTags.Count];
        for (int i = 0; i < foundedObjectsWithSelectedTags.Count; i++)
        {
            options[i] = foundedObjectsWithSelectedTags[i].name;                        // Fill the dropdown with objects names.



            if (script.target == foundedObjectsWithSelectedTags[i].transform)           // Keep the dropdown at the correct object if the camera is already following it.
            {
                selectedIndex = i;
            }
        }

        selectedIndex = EditorGUILayout.Popup("Camera Target", selectedIndex, options); // Show the dropdown menu for the found objects.

        // Protection of selecting a value outside the bounds.
        if (selectedIndex >= 0 && selectedIndex < foundedObjectsWithSelectedTags.Count) // If selected object is valid.
        {
            script.target = foundedObjectsWithSelectedTags[selectedIndex].transform;    // Return the position of the selected object to Camera_Flow.cs.
            EditorUtility.SetDirty(script);                                             // Save the selected object to follow when the project is closing down.
        }
    }
}
