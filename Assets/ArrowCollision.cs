using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ArrowCollision : MonoBehaviour
{
    [Header("Collision Settings")]
    public string targetTag = "Target";        // ટાર્ગેટનો tag// લાલ ટાર્ગેટનો tag
    public bool stopOnCollision = true;        // collision પર stop કરવું છે કે નહીં
    public bool destroyOnCollision = false;    // collision પર destroy કરવું છે કે નહીં
    public bool makeTargetKinematic = true;    // target ને પણ kinematic કરવું છે કે નહીં

    [Header("Visual Effects")]
    public bool showHitEffect = true;         // hit effect બતાવવું છે કે નહીં
    public Color hitColor = Color.red;         // hit થયા પછીનો રંગ
    private Rigidbody2D arrowRb;              // arrow નો rigidbody
    private SpriteRenderer arrowSprite;       // arrow નો sprite renderer

    public List<GameObject> targets = new List<GameObject>();

    public int Count = 0;

    public GameObject ArrowHitPrefab;

    void Start()
    {
        // Rigidbody2D અને SpriteRenderer get કરો
        arrowRb = GetComponent<Rigidbody2D>();
        arrowSprite = GetComponent<SpriteRenderer>();
    }


    void OnTriggerEnter2D(Collider2D other)
    {
        // જો target tag સાથે collide થાય તો
        if (other.gameObject.CompareTag(targetTag))
        {
            HandleCollision2D(other);
        }
    }

    void HandleCollision2D(Collider2D targetCollider)
    {

        Debug.Log("Arrow hit target - will not be destroyed");


        // Check arrow tag and update score accordingly
        if (targetCollider.CompareTag("Red"))
        {
            // Red arrow hit target - increase red score
            if (UIManager.Instance != null)
            {
                UIManager.Instance.AddRedScore(1);
                Debug.Log("Red Arrow hit! Red Score increased by 10");
            }
        }
        else if (targetCollider.CompareTag("Blue"))
        {
            // Blue arrow hit target - increase blue score
            if (UIManager.Instance != null)
            {
                UIManager.Instance.AddBlueScore(1);
                Debug.Log("Blue Arrow hit! Blue Score increased by 10");
            }
        }

        Vector2 Pos = targetCollider.transform.position;
        Pos.x -= 0.3f;
        GameObject New = Instantiate(ArrowHitPrefab, targetCollider.transform.position, Quaternion.identity);
        New.transform.SetParent(this.gameObject.transform);
        Destroy(targetCollider.gameObject);


        if (Count == 0)
        {
            if (targets.Count == 0)
            {
                targets.Add(New);
            }
            else
            {
                Destroy(targets[0]);
                targets.Remove(targets[0]);
                targets.Add(New);
            }
            Count++;
        }
        else if (Count == 1)
        {
            Destroy(targets[0]);
            targets.Remove(targets[0]);
            targets.Add(New);
            Count = 0;
        }

    }

}
