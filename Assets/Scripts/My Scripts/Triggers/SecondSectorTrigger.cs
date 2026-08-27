using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SecondSectorTrigger : MonoBehaviour
{
    [Header("Sector Settings")]
    public Transform tanksParent;
    public float respawnDelay = 15f;

    private Transform[] tankTransforms;
    private Vector3[] initialPositions;
    private Quaternion[] initialRotations;
    private bool[] isRespawning;
    
    private bool isPlayerInside = false;

    private float checkTimer = 0f;
    private float checkInterval = 1f; // Check every 1 second.

    void Start()
    {
        if (tanksParent != null)
        {
            int childCount = tanksParent.childCount;
            tankTransforms = new Transform[childCount];
            initialPositions = new Vector3[childCount];
            initialRotations = new Quaternion[childCount];
            isRespawning = new bool[childCount];

            for (int i = 0; i < childCount; i++)
            {
                tankTransforms[i] = tanksParent.GetChild(i);
                initialPositions[i] = tankTransforms[i].position;
                initialRotations[i] = tankTransforms[i].rotation;
                
                tankTransforms[i].gameObject.SetActive(false);
            }
        }
        else
        {
            Debug.LogError("[SecondSectorTrigger.cs] Parent of NPC Tanks is not assigned!");
        }
    }

    void Update()
    {
        if (!isPlayerInside || tankTransforms == null) return;

        checkTimer -= Time.deltaTime;

        if (checkTimer <= 0f) 
        {
            checkTimer = checkInterval;

            // Find dead tank and start respawn coroutine for the dead one.
            for (int i = 0; i < tankTransforms.Length; i++)
            {
                if (!tankTransforms[i].gameObject.activeInHierarchy && !isRespawning[i])
                {
                    StartCoroutine(RespawnTankCoroutine(i));
                }
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInside = true;
            
            if (tankTransforms != null)
            {
                for (int i = 0; i < tankTransforms.Length; i++)
                {
                    // Find all the tanks that are already dead and respawn them imidiately without delay.
                    if (!tankTransforms[i].gameObject.activeInHierarchy)
                    {
                        tankTransforms[i].position = initialPositions[i];
                        tankTransforms[i].rotation = initialRotations[i];
                        tankTransforms[i].gameObject.SetActive(true);
                    }
                }
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInside = false;
        }
    }

    private IEnumerator RespawnTankCoroutine(int index)
    {
        isRespawning[index] = true;                     // Check that sellected tank is in respawn proccess.

        yield return new WaitForSeconds(respawnDelay);

        
        if (isPlayerInside)                             // If player is still inside the sector, respawn the sellected tank.
        {
            tankTransforms[index].position = initialPositions[index];
            tankTransforms[index].rotation = initialRotations[index];
            tankTransforms[index].gameObject.SetActive(true);
        }

        isRespawning[index] = false;                    // Check that sellected tank is not in respawn proccess.
    }
}
