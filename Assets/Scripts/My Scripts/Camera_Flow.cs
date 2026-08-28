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
        // Calculate target camera possition.
        Vector3 desiredPosition = target.position + target.rotation * offset;

        // Initialize ray's parameters from player's tank towards camera.
        Vector3 rayStart = target.position + Vector3.up * 0.3f; 
        Vector3 direction = desiredPosition - rayStart;
        float maxDistance = direction.magnitude;

        // Shoot ray to find if there are any objects between player's tank and camera.
        RaycastHit[] hits = Physics.RaycastAll(rayStart, direction.normalized, maxDistance);
        
        float closestDistance = maxDistance;
        bool hitWall = false;
        Vector3 finalHitPoint = desiredPosition;

        foreach (RaycastHit hit in hits)
        {
            if (!hit.collider.CompareTag("Player") && !hit.collider.isTrigger)
            {
                if (hit.distance < closestDistance)
                {
                    closestDistance = hit.distance;
                    finalHitPoint = hit.point;
                    hitWall = true;
                }
            }
        }

        if (hitWall)
        {
            desiredPosition = finalHitPoint + (rayStart - finalHitPoint).normalized * 0.5f;
        }

        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothPossition * Time.deltaTime);

        // Smoothly rotate to match the target's rotation.
        Quaternion targetRotation = target.rotation;
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, smoothRotation * Time.deltaTime);
    }
}
