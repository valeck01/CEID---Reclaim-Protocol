using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Camera_Flow : MonoBehaviour
{
    public Transform target;

    // Offset from the target object.
    public static float offset_x = 0f;
    public static float offset_y = 7f;
    public static float offset_z = -40f;
    public Vector3 offset = new Vector3(offset_x, offset_y, offset_z);

    [Range(0.1f, 5f)] public float smoothPossition = 0.1f;
    [Range(0.1f, 5f)] public float smoothRotation = 0.1f;
    // Start is called before the first frame update
    void FixedUpdate()
    {
        // Smoothly follow the target position with offset.
        Vector3 desiredPosition = target.position + target.rotation * offset;
        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothPossition);

        // Smoothly rotate to match the target's rotation.
        Quaternion targetRotation = target.rotation;
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, smoothRotation);

        
    }
}
