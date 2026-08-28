using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sun_Behavior : MonoBehaviour
{
    [Header("Components")]
    public Transform sun_transform;
    private Light sun_light;
    
    [Header("Sun Parameters")]
    public float dayCycleInMinutes = 10f; // Let inspector assign how much minutes should pass till sun makes a full rotate.
    public float sunLightIntesityAtNight = 0f;

    private float rotationSpeed;
    private float defaultIntensity;

    void Start()
    {
        sun_transform = transform;
        sun_light = GetComponent<Light>();
        
        if (sun_light != null)
        {
            defaultIntensity = sun_light.intensity;
        }
        
        // Calculate needed speed: 360 angles / (minutes * 60 seconds)
        rotationSpeed = 360f / (dayCycleInMinutes * 60f);

        // Innitial sun's possition.
        sun_transform.position = new Vector3(0f, 0f, -100f); 
    }

    void Update()
    {
        // sun's rottation arround the scene.
        sun_transform.RotateAround(Vector3.zero, Vector3.right, rotationSpeed * Time.deltaTime);

        // Make sun to always look at the center of the map.
        sun_transform.LookAt(Vector3.zero);

        if (sun_light != null)
        {
            // if sun is under the scene. null it's light intensity.
            if (sun_transform.position.y < 0f)
            {
                sun_light.intensity = sunLightIntesityAtNight;
            }
            else
            {
                sun_light.intensity = defaultIntensity;
            }
        }
    }
}
