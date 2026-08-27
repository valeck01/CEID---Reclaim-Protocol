using System.Collections;
using UnityEngine;

public class NeutralSectorTrigger : MonoBehaviour
{
    [Header("Neutral Sector Settings")]
    public Transform tanksParent;
    public float respawnDelay = 15f;

    private Transform[] tankTransforms;
    private Vector3[] initialPositions;
    private Quaternion[] initialRotations;
    private bool[] isRespawning;

    private float checkTimer = 0f;
    private float checkInterval = 1f;

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
                
                // Be sure that all tanks in neutral sector are acrive.
                tankTransforms[i].gameObject.SetActive(true);
            }
        }
        else
        {
            Debug.LogError("[NeutralSectorTrigger.cs] Parent of NPC Tanks is not assigned!");
        }
    }

    void Update()
    {
        if (tankTransforms == null) return;

        checkTimer -= Time.deltaTime;

        if (checkTimer <= 0f)
        {
            checkTimer = checkInterval;

            // Find the dead tank and call respawn coroutine if needed.
            for (int i = 0; i < tankTransforms.Length; i++)
            {
                if (!tankTransforms[i].gameObject.activeInHierarchy && !isRespawning[i])
                {
                    StartCoroutine(RespawnTankCoroutine(i));
                }
            }
        }
    }

    private IEnumerator RespawnTankCoroutine(int index)
    {
        isRespawning[index] = true;

        yield return new WaitForSeconds(respawnDelay);

        tankTransforms[index].position = initialPositions[index];
        tankTransforms[index].rotation = initialRotations[index];
        tankTransforms[index].gameObject.SetActive(true);

        isRespawning[index] = false;
    }
}
