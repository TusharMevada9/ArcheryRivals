using System.Collections;
using UnityEngine;

public class ArrowShooter : MonoBehaviour
{
    [Header("Arrow Settings")]
    public GameObject arrowPrefab;         // તીરનો prefab
    public Transform shootPoint;          // તીર ક્યાંથી શૂટ થશે
    public float shootForce = 10f;        // તીર પર લગાવવાનું બળ
    public bool isLeft = true;            // ડાબી બાજુથી શૂટ કરવું છે કે જમણી બાજુથી

    [Header("Shooting Controls")]
    // Space key ઉપર તીર શૂટ થાય છે

    [Header("Arrow Physics")]
    public bool useGravity = false;       // gravity લાગવી છે કે નહીં (false = સીધું જાય)
    public float arrowLifetime = 5f;
    public bool resetVelocityOnSpawn = true; // spawn પર velocity reset કરવી છે કે નહીં
    public bool useDirectVelocity = true; // velocity directly set કરવી છે કે force લગાવવું છે


    void Start()
    {
        // જો shootPoint set નથી કર્યું તો bow ની position ઉપયોગ કરો
        if (shootPoint == null)
        {
            shootPoint = transform;
        }
    }
    public Vector3 spawnPosition;
    public bool isForceApplied = false;
    void Update()
    {
        // Space key ઉપર તીર શૂટ કરવા માટે

        if (isForceApplied == true)
        {
            isForceApplied = false;
            StartCoroutine(ShootArrow());
        }
        if (Input.GetKeyUp(KeyCode.Space))
        {
            isForceApplied = true;
        }

        spawnPosition = this.gameObject.transform.position;
    }

    // તીર શૂટ કરવાનું ફંક્શન
    public IEnumerator ShootArrow()
    {
        if (arrowPrefab == null)
        {
            Debug.LogWarning("Arrow Prefab is not assigned!");
            yield break;
        }

        // તીર instantiate કરો - exact position પર
        //Vector3 spawnPosition = shootPoint.position;
        GameObject newArrow = Instantiate(arrowPrefab, spawnPosition, Quaternion.identity);

        // Rigidbody2D મળે તો force લગાવો
        Rigidbody2D arrowRb = newArrow.GetComponent<Rigidbody2D>();
        if (arrowRb != null)
        {
            // પહેલા velocity zero કરો (જો setting ચાલુ હોય)
            if (resetVelocityOnSpawn)
            {
                arrowRb.linearVelocity = Vector2.zero;
                arrowRb.angularVelocity = 0f;
            }

            // isLeft bool પર આધારિત force direction
            Vector2 forceDirection;
            if (isLeft)
            {
                // ડાબી બાજુથી જમણી બાજુ
                forceDirection = new Vector2(1f, 0f); // જમણી બાજુ તરફ
            }
            else
            {
                // જમણી બાજુથી ડાબી બાજુ
                forceDirection = new Vector2(-1f, 0f); // ડાબી બાજુ તરફ
            }

            Debug.Log("Arrow shot " + (isLeft ? "left to right" : "right to left") + " with force: " + shootForce);

            if (useDirectVelocity)
            {
                // Velocity directly set કરો - વધુ reliable
                arrowRb.linearVelocity = forceDirection * shootForce;
                Debug.Log("Direct velocity set: " + arrowRb.linearVelocity);
            }
            else
            {
                arrowRb.AddForce(forceDirection * shootForce, ForceMode2D.Impulse);
                Debug.Log("Force applied: " + (forceDirection * shootForce));
            }

            Debug.Log("Arrow spawned at: " + spawnPosition + " with force: " + shootForce);
        }
        else
        {
            Debug.LogWarning("Arrow prefab doesn't have Rigidbody2D component!");
        }

        yield return new WaitForSeconds(3f);

        if (newArrow != null)
        {
            Destroy(newArrow,3f);
        }

        Debug.Log("Arrow Shot! Force: " + shootForce);
    }

    public void DestroyArrow()
    {
       

    }

}
