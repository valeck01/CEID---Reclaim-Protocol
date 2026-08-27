using UnityEngine;

public class BossGateTrigger : MonoBehaviour
{
    [Header("Gate Settings")]
    public Transform gatesObject;           // Let inspector assign Boss Gate Object.
    public float loweringSpeed = 3f;        // Let inspector assign opening speed.
    public float lowerByDistance = 15f;     // Let inspector assign how deep will gates go.

    private bool isPlayerNearBossGates = false;
    private bool isOpening = false;
    private Vector3 targetPosition;

    void Update()
    {
        if (isOpening)      // Note: Most often check.
        {
            gatesObject.position = Vector3.MoveTowards(gatesObject.position, targetPosition, loweringSpeed * Time.deltaTime);
            
            if (gatesObject.position == targetPosition)                         // destroy isNearBossGates game object trigger when gates are fully opened.
            {
                Debug.Log("The Boss Gates are opened!");
                Destroy(gameObject); 
            }
            return;                                                             // Stop the script here.
        }

        if (isPlayerNearBossGates && GameManager.Instance.bossGatesCanOpen)     // Note: One time check.
        {
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)) 
            {
                StartOpening();
            }
        }
    }

    void StartOpening()
    {
        isOpening = true;

        targetPosition = gatesObject.position - new Vector3(0, lowerByDistance, 0); // Calculate target possition for gates to drown.
        
        // Disable isNearBossGates game object trigger when gates starts to open.
        GetComponent<Collider>().enabled = false; 
        
        Debug.Log("The Boss Gates are opening!");
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) isPlayerNearBossGates = true;
        Debug.Log("[BossGateTrigger.cs] Player is near boss Gates.");
    }
    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player")) isPlayerNearBossGates = false;
        Debug.Log("[BossGateTrigger.cs] Player is far from boss Gates.");
    }
}