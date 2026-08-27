using System.Collections;
using UnityEngine;

public class SectorGateTrigger : MonoBehaviour
{
    [Header("Gate Settings")]
    public Transform gatesObject;           
    public float loweringSpeed = 3f;        
    public float lowerByDistance = 15f;     

    private Vector3 closedPosition;
    private Vector3 openedPosition;
    private Vector3 currentTargetPosition;

    private bool isPlayerInZone = false;
    private bool shouldOpen = false;

    void Start()
    {
        if (gatesObject == null)
        {
            Debug.LogError("[SectorGateTrigger.cs] Gates Object is not assigned!");
            return;
        }

        // Calculate Opened and Closed Possitions.
        closedPosition = gatesObject.position;
        openedPosition = closedPosition - new Vector3(0, lowerByDistance, 0);

        currentTargetPosition = closedPosition;

        StartCoroutine(CheckPlayerPresenceRoutine());       // Check Always woth 1 sec delay.
    }

    void Update()
    {
        if (gatesObject == null) return;

        if (shouldOpen)
        {
            currentTargetPosition = openedPosition;
        }
        else
        {
            currentTargetPosition = closedPosition;
        }

        if (gatesObject.position != currentTargetPosition)
        {
            // Move the gates.
            gatesObject.position = Vector3.MoveTowards(gatesObject.position, currentTargetPosition, loweringSpeed * Time.deltaTime);
        }
    }

    IEnumerator CheckPlayerPresenceRoutine()
    {
        while (true)
        {
            if (isPlayerInZone)
            {
                shouldOpen = true;
            }
            else
            {
                shouldOpen = false;
            }

            yield return new WaitForSeconds(1f); // wait for 1 second.
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInZone = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInZone = false;
        }
    }
}
