using System.Collections;
using UnityEngine;

public class BossSectorTrigger : MonoBehaviour
{
    [Header("Boss Gate Settings")]
    public Transform bossGateParent;        // Boss Sector's Gates.

    [Header("Boss Tanks Settings")]
    public GameObject bossTank1;            // First npc Boss Tank.
    public GameObject bossTank2;            // Second npc Boss Tank.
    public float tankRespawnTime = 30f;     // Respawn time for boss npc tanks.

    [Header("Boss Turrets Settings")]
    public Transform turretsParent;         // Empty game object Parent of all Turrets in boss sector.

    // Variables for respawn.
    private Vector3 gateInitialPos;
    private Quaternion gateInitialRot;

    private Vector3 tank1InitialPos;
    private Quaternion tank1InitialRot;
    private bool isTank1Respawning = false;

    private Vector3 tank2InitialPos;
    private Quaternion tank2InitialRot;
    private bool isTank2Respawning = false;

    private Transform[] turretTransforms;
    private Quaternion[] turretInitialRotations;
    private int totalTurrets;

    private bool battleStarted = false;
    private bool battleWon = false;

    void Start()
    {
        // Save Gates Initial Opened Position.
        if (bossGateParent != null)
        {
            gateInitialPos = bossGateParent.position;
            gateInitialRot = bossGateParent.rotation;
        }

        // Get tanks parameters from the scene and deactivate/hide them.
        if (bossTank1 != null)
        {
            tank1InitialPos = bossTank1.transform.position;
            tank1InitialRot = bossTank1.transform.rotation;
            bossTank1.SetActive(false);
        }
        if (bossTank2 != null)
        {
            tank2InitialPos = bossTank2.transform.position;
            tank2InitialRot = bossTank2.transform.rotation;
            bossTank2.SetActive(false);
        }

        // Get turrets parameters from the scene and deactivate/hide them.
        if (turretsParent != null)
        {
            totalTurrets = turretsParent.childCount;
            turretTransforms = new Transform[totalTurrets];
            turretInitialRotations = new Quaternion[totalTurrets];

            for (int i = 0; i < totalTurrets; i++)
            {
                turretTransforms[i] = turretsParent.GetChild(i);
                turretInitialRotations[i] = turretTransforms[i].rotation;
                turretTransforms[i].gameObject.SetActive(false);
            }
        }
        else 
        {
            Debug.Log("[BossSectorTrigger.cs] turrets parent is not assigned");
            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
            #endif
        }
    }

    void Update()
    {
        if (!battleStarted || battleWon) return;    // Do not run Update if battle does not started yet or already won.

        int activeTurrets = 0;                      // Counter for active turrets. All Script Logics are awfull so i cannot optimize this logic any further.
        if (turretTransforms != null)
        {
            // Count how much turrets in the sector is still active (parentGameObject).
            for (int i = 0; i < turretTransforms.Length; i++)
            {
                if (turretTransforms[i].gameObject.activeInHierarchy) activeTurrets++; 
            }
        }

        if (activeTurrets == 0)     // If none turrets are active, Check that the game is won.
        {
            battleWon = true;
            Debug.Log("All Boss Turrets are destroyed! Victory!");
            UIManager uiManager = FindObjectOfType<UIManager>();
            if (uiManager != null) uiManager.ShowVictoryPopup();
            return;
        }

        // If first boss tank is dead and not in respawn proccess.
        if (bossTank1 != null && !bossTank1.activeInHierarchy && !isTank1Respawning)
        {
            // Start timer for respawn.
            StartCoroutine(RespawnTankCoroutine(bossTank1, tank1InitialPos, tank1InitialRot, 1));
        }
        // If second boss tank is dead and not in respawn proccess.
        if (bossTank2 != null && !bossTank2.activeInHierarchy && !isTank2Respawning)
        {
            // Start timer for respawn.
            StartCoroutine(RespawnTankCoroutine(bossTank2, tank2InitialPos, tank2InitialRot, 2));
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !battleStarted)
        {
            battleStarted = true;

            // Close the Gates to trap the player.
            if (bossGateParent != null)
            {
                bossGateParent.position = gateInitialPos;
                bossGateParent.rotation = gateInitialRot;
            }

            // Spawn Boss Tanks.
            if (bossTank1 != null) bossTank1.SetActive(true);
            if (bossTank2 != null) bossTank2.SetActive(true);

            // Spawn Boss Turrets
            if (turretTransforms != null)
            {
                for (int i = 0; i < turretTransforms.Length; i++)
                {
                    turretTransforms[i].rotation = turretInitialRotations[i];
                    turretTransforms[i].gameObject.SetActive(true);
                }
            }
        }

    }

    private IEnumerator RespawnTankCoroutine(GameObject tank, Vector3 startPos, Quaternion startRot, int tankId)
    {
        // Check that Boss tanks are in Respawn proccess.
        if (tankId == 1) isTank1Respawning = true;
        if (tankId == 2) isTank2Respawning = true;

        yield return new WaitForSeconds(tankRespawnTime);

        // If player won the game stop coroutine. Note: this sould be here in case player wins when tanks are in respawn proccess.
        if (battleWon) yield break;

        // Reset Boss tank's possition/rotation and activate them. Note: Health_System.cs setting the health parameters automatically.
        tank.transform.position = startPos;
        tank.transform.rotation = startRot;
        tank.SetActive(true); 

        // Check that Boss tanks are in not in Respawn proccess anymore.
        if (tankId == 1) isTank1Respawning = false;
        if (tankId == 2) isTank2Respawning = false;
    }
}
