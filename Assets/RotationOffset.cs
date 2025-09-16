using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotationOffset : MonoBehaviour
{
    [Tooltip("Rotation offset in degrees (X, Y, Z).")]
    public Vector3 rotationOffset = Vector3.zero;

    void LateUpdate()
    {
        // Apply a fixed offset to the object's current rotation
        transform.rotation = transform.rotation * Quaternion.Euler(rotationOffset);
    }
}