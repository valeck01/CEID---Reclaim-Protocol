using UnityEngine;
using UnityEditor;

public class BulkPrefabReplacer : EditorWindow
{
    public GameObject oldPrefab;
    public GameObject newPrefab;

    [MenuItem("Tools/Μαζική Αλλαγή Prefabs")]
    public static void ShowWindow()
    {
        GetWindow<BulkPrefabReplacer>("Αλλαγή Prefabs");
    }

    void OnGUI()
    {
        GUILayout.Label("Ρυθμίσεις Αντικατάστασης", EditorStyles.boldLabel);

        oldPrefab = (GameObject)EditorGUILayout.ObjectField("1. Παλιό Prefab (Από τους Φακέλους):", oldPrefab, typeof(GameObject), false);
        newPrefab = (GameObject)EditorGUILayout.ObjectField("2. Νέο Prefab (Από τους Φακέλους):", newPrefab, typeof(GameObject), false);

        if (GUILayout.Button("Αντικατάσταση Όλων στη Σκηνή!"))
        {
            if (oldPrefab == null || newPrefab == null)
            {
                Debug.LogWarning("Παρακαλώ βάλε και το Παλιό και το Νέο Prefab.");
                return;
            }

            ReplacePrefabsInScene();
        }
    }

    void ReplacePrefabsInScene()
    {
        // Βρίσκει όλα τα αντικείμενα στη σκηνή
        GameObject[] allObjects = FindObjectsOfType<GameObject>();
        int replacedCount = 0;

        foreach (GameObject obj in allObjects)
        {
            // Αν το αντικείμενο είναι αντίγραφο του ΠΑΛΙΟΥ Prefab
            if (PrefabUtility.GetCorrespondingObjectFromOriginalSource(obj) == oldPrefab)
            {
                // Δημιουργούμε το νέο Prefab
                GameObject newInstance = (GameObject)PrefabUtility.InstantiatePrefab(newPrefab);
                
                // Αντιγράφουμε θέση, γωνία, μέγεθος, και "γονέα"
                newInstance.transform.SetPositionAndRotation(obj.transform.position, obj.transform.rotation);
                newInstance.transform.localScale = obj.transform.localScale;
                newInstance.transform.parent = obj.transform.parent;

                // Σβήνουμε το παλιό
                Undo.RegisterCreatedObjectUndo(newInstance, "Replace Prefabs");
                Undo.DestroyObjectImmediate(obj);
                
                replacedCount++;
            }
        }

        Debug.Log($"Ολοκληρώθηκε! Αντικαταστάθηκαν {replacedCount} αντικείμενα.");
    }
}