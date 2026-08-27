using System;
using UnityEngine;

public class New_Tank_Movement : MonoBehaviour
{
    private float axisX;
    private float axisZ;

    [Header("Movement Parameters")]                      // Show in the Inspector.
    public float Max_Tank_Speed;
    public float Max_Turn_Speed;
    [Range(0f, 1f)] public float speed_Smoothness;
    public bool isEngineOn = true;                      // Turn On/Off the engine via keyboard Button.

    /*
    [Header("Spawn Settings")]
    public Vector3 initialPosition;                             // Let Inspector to set the starting Position of the tank.
    public Vector3 initialRotation;                             // Let Inspector to set the starting Rotation of the tank.
    */

    [Header("Engine Audio")]                                    
    [SerializeField] private AudioSource audioSource;           // Engine's Audio Source.
    [SerializeField] private AudioClip engineIdleClip;          // Let Inspector to set the EngineIdle clip.
    [SerializeField] private AudioClip engineDrivingClip;       // EngineDriving clip.
    public float pitchRange = 0.2f;                             // Pitch Range.
    private float originalPitch;                                // Original pitch of the AudioSource.

    private Rigidbody rb;                   
    private CapsuleCollider capsuleCollider;

    void Start()
    {
        // Get all components by reference.
        rb = GetComponent<Rigidbody>();
        capsuleCollider = GetComponent<CapsuleCollider>();

        //Check if Rigidbody is assigned correctly.
        if (rb == null)
        {
            Debug.LogError("Rigidbody for tank is not assigned!");
            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;    // If running in the Unity Editor.
            #endif
        }

        // Lock some parameters to ensure that the object can not pass throu other objects.
        rb.isKinematic = false;
        rb.useGravity = false;

        // Lock the object because we move in 2D dimension.
        rb.constraints = RigidbodyConstraints.FreezeRotationX |     //Lock the X rotation of the tank.
                         RigidbodyConstraints.FreezeRotationZ |     //Lock the Z rotation of the tank.
                         RigidbodyConstraints.FreezePositionY;      //Lock the Y position of the tank.
        
        capsuleCollider.enabled = true;
        capsuleCollider.isTrigger = false;
        capsuleCollider.center = new Vector3(0.0f, 1.60f, -0.1f);
        capsuleCollider.radius = 1.1f;
        capsuleCollider.height = 3.15f;
        capsuleCollider.direction = 2;

        //Check if Capsule Collider is assigned correctly.
        if (capsuleCollider == null)
        {
            Debug.LogError("Capsule Collider for tank is not assigned!");
            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;    // If running in the Unity Editor.
            #endif
        }

        //Check if Audio Source is assigned correctly.
        if (audioSource == null)
        {
            Debug.LogError("Audio Source for tank engine is not assigned!");
            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;    // If running in the Unity Editor.
            #endif
        }

        // Start the engine sound and set it to loop.
        originalPitch = audioSource.pitch;                          // Apply Inspectors initial Pitch.
        audioSource.clip = engineIdleClip;                          // Set the engineIdle sound to the one set in the Inspector.
        audioSource.loop = true;                                    // Set the engineIdle sound to loop.
        audioSource.Play();                                         // Play the engineIdle sound.

    }

    void Update()
    {
        axisX = Input.GetAxis("Horizontal1");   // Turning Left/Right.
        axisZ = Input.GetAxis("Vertical1");     // Forward/Backward Movement.

        if (axisZ < 0)
        {
            axisZ *= 0.5f;                      // Apply half speed when going backwards.
            axisX = -axisX;                     // Reverse horizontal axis when going backwards.
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            isEngineOn = !isEngineOn;           // Turn On/Off tank's engine.
        }

        if (!isEngineOn)                        // Turn off movement imput if engine is off
        {
            axisX = 0f;                         
            axisZ = 0f;                         
        }
    }
    void FixedUpdate()
    {
        // Move tank forward/backward.
        Vector3 desiredVelocity = axisZ * Max_Tank_Speed * transform.forward;
        rb.velocity = Vector3.Lerp(rb.velocity, desiredVelocity, speed_Smoothness);

        // Turn tank left/right.
        Vector3 desiredTurnVelocity = axisX * Max_Turn_Speed * Vector3.up;
        Quaternion turnRotation = Quaternion.Euler(desiredTurnVelocity * Time.fixedDeltaTime);
        rb.MoveRotation(rb.rotation * turnRotation);

        
        EngineAudio(); // Let EngineAudio function to handle the sound when tank is mooving.

        /*
        // Handle engine sound based on movement.
        float desiredAudioVolume = MathF.Max(Mathf.Abs(axisZ), Mathf.Abs(axisX)) / 2f;              // Volume based on movement input.
        audioSource.volume = Mathf.Lerp(audioSource.volume, desiredAudioVolume, speed_Smoothness);  // Volume based on forward/backward input.
        */
    }

    // Start of my Functions.============================
    private void EngineAudio()
    {
        bool isMoving = Mathf.Abs(axisZ) > 0.1f || Mathf.Abs(axisX) > 0.1f;     // Check if tank is mooving.

        // Change between Engine Idle and Engine Driving.
        if (isMoving)                                                           // Tank is Mooving.
        {
            if (audioSource.clip == engineIdleClip)                               
            {
                audioSource.clip = engineDrivingClip;                           // Change to Driving Clip.
                audioSource.Play();
            }
        }
        else                                                                    // Tank does not Mooving.
        {
            if (audioSource.clip == engineDrivingClip)
            {
                audioSource.clip = engineIdleClip;                              // Change to Idle Clip.
                audioSource.Play();
            }
        }

        float movementMagnitude = Mathf.Max(Mathf.Abs(axisZ), Mathf.Abs(axisX));            // Calculate the audios volume based on speed.

        float targetPitch = originalPitch + (movementMagnitude * pitchRange);               // Calculate target pitch for effect of speeding up.
        audioSource.pitch = Mathf.Lerp(audioSource.pitch, targetPitch, speed_Smoothness);   // Set calculated pitch.

        // Smooth changing the volume of the audio source.
        float targetVolume;
        if (!isEngineOn)
        {
             targetVolume = 0.0f;       // Turn off engine's volume if engine is off.
        }
        else if (isMoving)
        {
            targetVolume = 0.5f;        // If engine is on and tank is mooving play enginDriving.audio at 50% volume.
        }
        else
        {
            targetVolume = 0.2f;        // If engine is on and tank is not mooving, play engineIdle.audio at 20% volume.
        }
        
        // Calculate and change the volume smoothly with lerp function.
        audioSource.volume = Mathf.Lerp(audioSource.volume, targetVolume, speed_Smoothness);
        if (audioSource.volume < 0.1f)
        {
            audioSource.enabled = false;
        }
        else
        {
            if (audioSource.enabled == false) 
            {
                audioSource.enabled = true; 
                audioSource.Play();         
            }
        }
    }
    // End of my Functions.==============================
}
