using System;
using UnityEngine;

public class ArrowStop : MonoBehaviour
{
    public bool isRed;

    void OnTriggerEnter2D(Collider2D other)
    {

        if (isRed)
        {
            if (other.gameObject.CompareTag("Blue"))
            {
                Rigidbody2D arrowRb = other.GetComponent<Rigidbody2D>();
                if (arrowRb != null)
                {
                    arrowRb.linearVelocity = Vector2.zero;
                    arrowRb.angularVelocity = 0f;
                    other.GetComponent<BoxCollider2D>().enabled = false;
                    other.GetComponent<Arrow>().isRotate = true;

                    // Set gravity scale to 1
                    arrowRb.gravityScale = 2f;
                }

            }
        }
        else
        {
            if (other.gameObject.CompareTag("Red"))
            {
                Rigidbody2D arrowRb = other.GetComponent<Rigidbody2D>();
                if (arrowRb != null)
                {
                    arrowRb.linearVelocity = Vector2.zero;
                    arrowRb.angularVelocity = 0f;
                    other.GetComponent<BoxCollider2D>().enabled = false;
                    other.GetComponent<Arrow>().isRotate = true;

                    // Set gravity scale to 1
                    arrowRb.gravityScale = 2f;
                }

            }
        }
    }
}
