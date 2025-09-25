using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ArrowCollision : MonoBehaviour
{
    [Header("Collision Settings")]
    public string targetTag = "Target";
    public bool stopOnCollision = true;
    public bool destroyOnCollision = false;
    public bool makeTargetKinematic = true;

    [Header("Visual Effects")]
    public bool showHitEffect = true;
    public Color hitColor = Color.red;
    private Rigidbody2D arrowRb;
    private SpriteRenderer arrowSprite;

    public List<GameObject> targets = new List<GameObject>();

    public int Count = 0;

    public GameObject ArrowHitPrefab;


    public GameObject Particals;

    void Start()
    {
        arrowRb = GetComponent<Rigidbody2D>();
        arrowSprite = GetComponent<SpriteRenderer>();
    }


    void OnTriggerEnter2D(Collider2D other)
    {
        if (UIManager.Instance.isGameStart == true)
        {
            if (other.gameObject.CompareTag(targetTag))
            {
                HandleCollision2D(other);
            }
        }
    }

    void HandleCollision2D(Collider2D targetCollider)
    {

        Debug.Log("Arrow hit target - will not be destroyed");

        // Play arrow hit target SFX (singleplayer)
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayRandomArrowHitTarget();
        }
        Particals.SetActive(true);
        Particals.GetComponent<ParticleSystem>().Play();


        if (targetCollider.CompareTag("Red"))
        {
            if (UIManager.Instance != null)
            {
                UIManager.Instance.AddRedScore(1);
                Debug.Log("Red Arrow hit! Red Score increased by 10");
            }
        }
        else if (targetCollider.CompareTag("Blue"))
        {
            if (UIManager.Instance != null)
            {
                UIManager.Instance.AddBlueScore(1);
                Debug.Log("Blue Arrow hit! Blue Score increased by 10");
            }
        }

        Vector2 Pos = targetCollider.transform.position;
        //Pos.x -= 0.35f;
        GameObject New = Instantiate(ArrowHitPrefab, targetCollider.transform.position, Quaternion.identity);
        New.transform.SetParent(this.gameObject.transform);
        if (targetCollider.CompareTag("Red"))
        {
            New.transform.localPosition = new Vector2(-0.38f, New.transform.localPosition.y);
        }
        else
        {
            New.transform.localPosition = new Vector2(0.38f, New.transform.localPosition.y);

        }

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

        Invoke(nameof(ParticalFalse), 1f);
    }


    public void ParticalFalse()
    {
        Particals.GetComponent<ParticleSystem>().Stop();
        Particals.SetActive(false);
    }

}
