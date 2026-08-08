using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sun_Behavior : MonoBehaviour
{
    [Header("Components")]
    public Transform sun_transform;
    
    [Header("Sun Parameters")]
    public float rotationSpeed =2f; // degrees per second.

    // Start is called before the first frame update
    void Start()
    {
        sun_transform = transform;
        
        // Initialize Sun Parameters.
        sun_transform.position = new Vector3(0f, 0f, -100f); // Start position of the sun.

    }

    // Update is called once per frame
    void Update()
    {
        // Rotate the sun around the center of the scene (x-axis,0,0).
        sun_transform.RotateAround(Vector3.zero, Vector3.right, rotationSpeed * Time.deltaTime);

        // Keep the sun always facing the center of the scene.
        sun_transform.LookAt(Vector3.zero);
    }
}
