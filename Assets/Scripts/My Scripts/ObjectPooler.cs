using System.Collections.Generic;
using UnityEngine;

public class ObjectPooler : MonoBehaviour
{
    public static ObjectPooler Instance;

    [Header("Pool Settings")]
    public GameObject objectToPool;                 // Let Inspector to choose the Asset for shells.
    public int amountToPool = 20;                  // Let Inspector to choose how many shells to pre-compile.

    private List<GameObject> pooledObjects;         // The pool With all the available shells.

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        // Initialize the pool with shells.
        pooledObjects = new List<GameObject>();             
        for (int i = 0; i < amountToPool; i++)
        {
            GameObject obj = Instantiate(objectToPool);     // Create a shell.
            obj.transform.SetParent(this.transform);        // Set the new shell as child of the ObjectPooler.
            obj.SetActive(false);                           // Shells has to be inactive at the start.
            pooledObjects.Add(obj);                         // Add the shell to the pool.
        }
    }

    public GameObject GetPooledObject()                     // Call this function to get an available shell.
    {
        for (int i = 0; i < pooledObjects.Count; i++)       // Check for an available shell.
        {
            if (!pooledObjects[i].activeInHierarchy)        // If an available shell is found.
            {
                return pooledObjects[i];                    // Return the shell.
            }
        }

        // Those commands will run only if no available shells are left.
        Debug.LogWarning("ObjectPooler: Out of objects in the pool. Creating a new one.");
        GameObject obj = Instantiate(objectToPool);         // Create one more shell.
        obj.transform.SetParent(this.transform);            // Set the new shell as child of the ObjectPooler.
        obj.SetActive(false);                               // Deactivate the shell.
        pooledObjects.Add(obj);                             // Add the shell to the pool.
        return obj;                                         // Return the shell.
    }
}
