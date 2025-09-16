using JetBrains.Annotations;
using Unity.VisualScripting;
using UnityEngine;

public class Arrow : MonoBehaviour
{
    public bool isRotate = false;
    public float rotationSpeed = 100f; // Rotation speed in degrees per second
    
    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (isRotate && rb != null)
        {
            // Rotate the arrow based on its speed
            float speed = rb.linearVelocity.magnitude;
            float rotationAmount = speed * rotationSpeed * Time.deltaTime;
            
            // Rotate around Z-axis
            transform.Rotate(0, 0, rotationAmount);
        }
    }
}
