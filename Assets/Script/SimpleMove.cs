using UnityEngine;

public class SimpleMove : MonoBehaviour
{
    [Header("Direction & Speed")]
    public Vector3 direction = Vector3.forward; // set (x,y,z) in Inspector
    public float speed = 3f;                     // units per second

    void Update()
    {
        transform.position += direction.normalized * speed * Time.deltaTime;
    }
}

