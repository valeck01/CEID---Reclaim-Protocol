using UnityEngine;

public class BaseArea : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            GameManager.Instance.isPlayerInBase = true;     // Update GameManager.cs knowledge about player's location.
            Debug.Log("[BaseArea.cs] Player entered in the Base");
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            GameManager.Instance.isPlayerInBase = false;    // Update GameManager.cs knowledge about player's location.
            Debug.Log("[BaseArea.cs] Player exit from the Base");
        }
    }
}
