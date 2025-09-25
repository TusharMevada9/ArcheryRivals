using System;
using UnityEngine;

public class ArrowStop : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other)
    {
        Rigidbody2D arrowRb = other.GetComponent<Rigidbody2D>();
        if (arrowRb != null)
        {
            arrowRb.linearVelocity = Vector2.zero;
            arrowRb.angularVelocity = 0f;
            other.GetComponent<BoxCollider2D>().enabled = false;
           // other.GetComponent<CircleCollider2D>().enabled = false;
            other.GetComponent<Arrow>().isRotate = true;

            // Set gravity scale to 1
            arrowRb.gravityScale = 2f;
        }

        // Play random arrow hit line sound
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayRandomArrowHitLine();
        }

        var shooter = FindFirstObjectByType<AIArrowShooter>();
        if (shooter != null)
            shooter.NotifyAIArrowHit();

    }
}
