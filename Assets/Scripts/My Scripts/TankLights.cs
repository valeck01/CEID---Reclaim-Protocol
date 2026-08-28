using UnityEngine;

public class TankLights : MonoBehaviour
{
    [Header("Headlights Settings")]
    public Light[] headLights;      // Let inspector assign light's game objects.

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.L))
        {
            if (headLights != null)
            {
                // Toogle Lights.
                foreach (Light light in headLights)
                {
                    light.enabled = !light.enabled;
                }
            }
        }
    }
}
