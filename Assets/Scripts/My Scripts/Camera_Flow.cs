using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Camera_Flow : MonoBehaviour
{
    public Transform target;

    // Offset from the target object.

    public Vector3 offset = new Vector3();

    [Range(0.1f, 5f)] public float smoothPossition;
    [Range(0.1f, 5f)] public float smoothRotation;
    // Start is called before the first frame update
    void LateUpdate()
    {
        // Smoothly follow the target position with offset.
        Vector3 desiredPosition = target.position + target.rotation * offset;
        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothPossition * Time.deltaTime);

        // Smoothly rotate to match the target's rotation.
        Quaternion targetRotation = target.rotation;
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, smoothRotation * Time.deltaTime);
    }
}
