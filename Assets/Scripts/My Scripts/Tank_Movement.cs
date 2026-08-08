using System;
using UnityEngine;

public class Tank_Movement : MonoBehaviour
{
    [Header("Tank Parameters")]
    public float Max_Tank_Speed;
    public float speed_Difference;
    public float Max_Turn_Speed;

    [Header("Components")]
    private Rigidbody rb;
    private CapsuleCollider capsuleCollider;

    [Header("Audio Components")]
    private AudioSource audioSource;
    
    void Start()
    {   
        // Initialize Audio Source.
        audioSource = GetComponent<AudioSource>();
        audioSource.clip = Resources.Load<AudioClip>("AudioClips/EngineDriving");   // Load Engine Driving sound.

        //Check if Audio Source is assigned correctly.
        if (audioSource == null)
        {
            Debug.LogError("Audio Source for tank engine is not assigned properly!");
            UnityEditor.EditorApplication.isPlaying = false;    // If running in the Unity Editor.
        }

        audioSource.volume = 0.2f;                                                  // Adjust volume as needed.
        audioSource.loop = true;                                                    // Loop the engine sound.
        audioSource.playOnAwake = true;                                             // Play on awake.
        audioSource.spatialBlend = 1f;                                              // 3D sound.
        audioSource.pitch = 1f;                                                     // Normal pitch.
        audioSource.rolloffMode = AudioRolloffMode.Linear;                          // Linear volume rolloff.
        audioSource.maxDistance = 100f;                                             // Max distance for sound audibility.
        audioSource.minDistance = 1f;                                               // Min distance for sound audibility.
        
        // Initialize Vehicle's Rigidbody.
        rb = GetComponent<Rigidbody>();
        rb.mass = 1000f;
        rb.drag = 10f;
        rb.angularDrag = 10f;
        rb.isKinematic = false;
        rb.useGravity = false;
        rb.constraints = 
        (
            RigidbodyConstraints.FreezeRotationX |
            RigidbodyConstraints.FreezeRotationZ |
            RigidbodyConstraints.FreezePositionY
        );

        // Initialize Vehicle Parameters.
        Max_Tank_Speed      = 100f;      // max forward speed (units/sec).
        speed_Difference    = 0.1f;      // smoothing for velocity changes
        Max_Turn_Speed      = 60f;       // degrees/sec rotation speed.

        // Initialize Vehicle's CapsuleCollider.
        capsuleCollider = GetComponent<CapsuleCollider>();
        capsuleCollider.enabled     = true;
        capsuleCollider.isTrigger   = false;
        capsuleCollider.center      = new Vector3(0f, 1.60f, -0.1f);
        capsuleCollider.radius      = 1.10f;
        capsuleCollider.height      = 3.15f;
        capsuleCollider.direction   = 2; // Z-axis

        // Initialize Start Location & Rotation.
        transform.position = new Vector3(-165f, 0f, -180f);
        transform.rotation = Quaternion.Euler(0f, 0f, 0f);
    }
    
    void Update()
    {
        
    }
    void FixedUpdate()
    {
        float axisX = Input.GetAxis("Horizontal1"); // Turning Horizontal.
        float axisZ = Input.GetAxis("Vertical1");   // Forward/Backward Movement.

        if (axisZ < 0)
        {
            axisZ *= 0.5f;      // Apply half speed when going backwards.
            axisX = -axisX;     // Reverse horizontal axis when going backwards.
        }

        // Move tank forward/backward.
        Vector3 desiredVelocity = axisZ * Max_Tank_Speed * transform.forward;
        rb.velocity = Vector3.Lerp(rb.velocity, desiredVelocity, speed_Difference);

        // Turn tank left/right.
        Vector3 desiredTurnVelocity = axisX * Max_Turn_Speed * Vector3.up;
        Quaternion turnRotation = Quaternion.Euler(desiredTurnVelocity * Time.fixedDeltaTime);
        rb.MoveRotation(rb.rotation * turnRotation);

        // Handle engine sound based on movement.
        float desiredAudioVolume = MathF.Max(Mathf.Abs(axisZ), Mathf.Abs(axisX)) / 2f;              // Volume based on movement input.
        audioSource.volume = Mathf.Lerp(audioSource.volume, desiredAudioVolume, speed_Difference);  // Volume based on forward/backward input.

        
        /*
        // Handle engine sound based on movement.
        if (Mathf.Abs(axisZ) > 0.05f || Mathf.Abs(axisX) > 0.05f)
        {
            float desiredAudioVolume = MathF.Max(Mathf.Abs(axisZ), Mathf.Abs(axisX));                   // Volume based on movement input.
            audioSource.volume = Mathf.Lerp(audioSource.volume, desiredAudioVolume, speed_Difference);  // Volume based on forward/backward input.
        }
        else audioSource.volume = 0f;                                                                   // Volume based on forward/backward input.
        */
        
    }
}
