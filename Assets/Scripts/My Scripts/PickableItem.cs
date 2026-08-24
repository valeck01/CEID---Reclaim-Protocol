using System.Collections;
using UnityEngine;

public class PickableItem : MonoBehaviour
{
    // Initialize Item Types.
    public enum ItemType 
    { 
        RepairType, 
        MovementSpeedType, 
        ReloadBuffType, 
        LoreItemType, 
        BossKeyItemType 
    }

    [Header("Item Settings")]
    public ItemType myItemType;         // Let inspector assign pickable's object type from dropdown menu.
    public float respawnTime = 10f;     // Let inspector assign respawn time for items in minutes.
    public int loreID = 0;              // Let inspector assing the lore item's ID

    [Header("Animation Settings")]
    public float rotationSpeed = 10f;   // Let inspector assign rotationSpeed.
    public float floatSpeed = 2f;       // Let inspector assign up/down speed movement.
    public float floatHeight = 0.5f;    // Let inspector assign Max height for up/down movement.

    [Header("References")]
    private Vector3 initialPosition;    // Get items position.
    private Light pointLight;           // Get light component.
    private Collider itemCollider;      // Get item's collider component.
    private Renderer[] allRenderers;    // Get items renderers (item's Graphics)

    void Start()
    {
        initialPosition = transform.position;   // Save Item's Possition from Inspector.

        // Get item's components.
        pointLight = GetComponentInChildren<Light>();
        itemCollider = GetComponent<Collider>();
        allRenderers = GetComponentsInChildren<Renderer>();

        // Check if all components are assigned.
        if (pointLight == null) 
        {
            Debug.LogError($"[PickableItem] Game Object {gameObject.name} does not have Light component.");
            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
            #endif
        }

        if (itemCollider == null) 
        {
            Debug.LogError($"[PickableItem] Game Object {gameObject.name} does not have Collider component.");
            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
            #endif
        }
        else if (!itemCollider.isTrigger)
        {
            Debug.LogError($"[PickableItem] Game Object's {gameObject.name} Collider has to have enabled 'Is Trigger' setting!");
            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
            #endif
        }

        if (allRenderers.Length == 0)
        {
            Debug.LogError($"[PickableItem] Game Object {gameObject.name} does not have MeshRenderer! (It's invisible in the map).");
            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
            #endif
        }

    }

    void Update()
    {   
        // Always Rotate.
        transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime, Space.World);

        // Always float Up/Down.
        float newY = Mathf.Sin(Time.time * floatSpeed) * floatHeight;
        transform.position = new Vector3(initialPosition.x, initialPosition.y + newY, initialPosition.z);
    }

    // Start of my functions ---------------------------

    void OnTriggerEnter(Collider other)
    {
        // Check if Player Tank's collider triggered item's collider.
        if (other.CompareTag("Player"))
        {
            // Search Player's Inventory.
            PlayerInventory inventory = other.GetComponent<PlayerInventory>();
            
            // If player has inventory.
            if (inventory != null)
            {
                // Call inventory's function and translate item's Type to string, otherwise this function will run default case.
                inventory.AddItem(myItemType.ToString(), loreID);
                // If item's type is unique then just destroy them.
                if (myItemType == ItemType.LoreItemType || myItemType == ItemType.BossKeyItemType)
                {
                    // Τα Ειδικά αντικείμενα εξαφανίζονται οριστικά από το χάρτη
                    Destroy(gameObject); 
                }
                else // If item type is buff type then just hide them from the game with delay.
                {
                    StartCoroutine(RespawnRoutine());
                }
            }
        }
    }

    // 
    private IEnumerator RespawnRoutine()
    {
        // Disable item's collider.
        itemCollider.enabled = false;
        
        // Disable Item's Light.
        pointLight.enabled = false;
        
        // Disable Item's renderer to make it invisible in the game.
        foreach (Renderer r in allRenderers)
        {
            r.enabled = false;
        }

        // Wait before re-enable  items component's
        yield return new WaitForSeconds(respawnTime);

        // Re-enable item's components
        itemCollider.enabled = true;
        pointLight.enabled = true;
        foreach (Renderer r in allRenderers)
        {
            r.enabled = true;
        }
    }
    

    // End of my functions -----------------------------




}
