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

    /*
    [Header("Spawn Settings")]
    public Vector3 initialPosition;                             // Let Inspector to set the starting Position of the tank.
    public Vector3 initialRotation;                             // Let Inspector to set the starting Rotation of the tank.
    */

    [Header("Engine Audio")]                                    
    [SerializeField] private AudioSource audioSource;           // Let Inspector choose Audio Source.
    [SerializeField] private AudioClip engineSound;             // Let Inspector choose Engine Sound.

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
        audioSource.clip = engineSound;                             // Set the engine sound to the one set in the Inspector.
        audioSource.loop = true;                                    // Set the engine sound to loop.
        audioSource.Play();                                         // Play the engine sound.

    }

    void Update()
    {
        axisX = Input.GetAxis("Horizontal1"); // Turning Left/Right.
        axisZ = Input.GetAxis("Vertical1");   // Forward/Backward Movement.

        if (axisZ < 0)
        {
            axisZ *= 0.5f;      // Apply half speed when going backwards.
            axisX = -axisX;     // Reverse horizontal axis when going backwards.
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

        // Handle engine sound based on movement.
        float desiredAudioVolume = MathF.Max(Mathf.Abs(axisZ), Mathf.Abs(axisX)) / 2f;              // Volume based on movement input.
        audioSource.volume = Mathf.Lerp(audioSource.volume, desiredAudioVolume, speed_Smoothness);  // Volume based on forward/backward input.
    }
}
