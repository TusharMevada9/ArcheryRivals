using System.Collections;
using UnityEngine;

public class AIArrowShooter : MonoBehaviour
{
    [Header("AI Arrow Settings")]
    public GameObject arrowPrefab;
    public Transform shootPoint;
    public float shootForce = 8f;
    public float shootInterval = 2f;
    public bool aiAutoShoot = true;

    [Header("Transform References")]
    public Transform Bow; // First transform reference
    public Transform Target; // Second transform reference

    [Header("Distance Settings")]
    public float maxShootDistance = 3f; // Maximum distance to shoot

    [Header("Arrow Physics")]
    public bool useGravity = false;
    public float arrowLifetime = 3f;
    public bool resetVelocityOnSpawn = true;
    public bool useDirectVelocity = true;

    [Header("AI Difficulty Levels")]
    public AIMode currentAIDifficulty = AIMode.Easy;

    private Vector3 spawnPosition;
    private float currentDistance = 0f;
    private bool canShoot = false;

    public bool isGameStart = false;

    public GameObject BowClickImage;
    public GameObject BowNoClickImage;

    [Header("AI Image Switching")]
    public float imageSwitchDuration = 0.5f; // Duration to show BowClickImage when shooting

    void Start()
    {
        if (shootPoint == null)
        {
            shootPoint = transform;
        }

        if (IFrameBridge.Instance.botLevel == AIMode.Easy)
        {
            currentAIDifficulty = AIMode.Easy;
            shootInterval = 2f;
        }
        else
        {
            currentAIDifficulty = AIMode.Hard;
            shootInterval = 1.5f;
        }

        // Assign blue target when AI spawns
        AssignBlueTarget();

        StartAiShooting();
    }

    void Update()
    {
        if (UIManager.Instance.isGameStart == true)
        {
            spawnPosition = this.gameObject.transform.position;

            isGameStart = UIManager.Instance.isGameStart;

            if (Bow != null && Target != null)
            {
                float yDiff = Bow.position.y - Target.position.y;
                currentDistance = Mathf.Abs(yDiff);

                if (currentDistance >= 0f && currentDistance <= 1f)
                {
                    canShoot = true;
                }
                else if (currentDistance >= 1f && currentDistance <= 4f)
                {
                    canShoot = true;
                }
                else
                {
                    canShoot = false;
                }
            }
            else
            {
                canShoot = false;
            }
        }
    }

    void StartAiShooting()
    {
        Invoke(nameof(AIShootCycle), shootInterval);
    }

    void AIShootCycle()
    {
        if (!aiAutoShoot) return;

        if (canShoot)
        {
            Debug.Log($"{currentAIDifficulty} AI - Distance {currentDistance:F2} <= {maxShootDistance}. Shooting now!");
            StartCoroutine(ShootArrow());
        }
        else
        {
            Debug.Log($"{currentAIDifficulty} AI - Distance {currentDistance:F2} > {maxShootDistance}. Waiting...");
        }

        Invoke(nameof(AIShootCycle), shootInterval);
    }
    public IEnumerator ShootArrow()
    {
        BowClickImage.SetActive(true);
        BowNoClickImage.SetActive(false);

        if (arrowPrefab == null)
        {
            Debug.LogWarning("AI Arrow Prefab is not assigned!");
            yield break;
        }
        
        yield return new WaitForSeconds(0.5f);
           
       
        GameObject newArrow = Instantiate(arrowPrefab, spawnPosition, Quaternion.identity);

        Rigidbody2D arrowRb = newArrow.GetComponent<Rigidbody2D>();
        if (arrowRb != null)
        {
            if (resetVelocityOnSpawn)
            {
                arrowRb.linearVelocity = Vector2.zero;
                arrowRb.angularVelocity = 0f;
            }

            Vector2 forceDirection = new Vector2(-1f, 0f);

            Debug.Log("AI Arrow shot right to left with force: " + shootForce);

            if (useDirectVelocity)
            {
                arrowRb.linearVelocity = forceDirection * shootForce;
                Debug.Log("AI Direct velocity set: " + arrowRb.linearVelocity);
            }
            else
            {
                arrowRb.AddForce(forceDirection * shootForce, ForceMode2D.Impulse);
                Debug.Log("AI Force applied: " + (forceDirection * shootForce));
            }

            Debug.Log("AI Arrow spawned at: " + spawnPosition + " with force: " + shootForce);
        }
        else
        {
            Debug.LogWarning("AI Arrow prefab doesn't have Rigidbody2D component!");
        }

        BowClickImage.SetActive(false);
        BowNoClickImage.SetActive(true);

        yield return new WaitForSeconds(arrowLifetime);

        if (newArrow != null)
        {
            Destroy(newArrow);
        }

    }

    public void AssignBlueTarget()
    {
        GameObject blueTarget = GameObject.FindGameObjectWithTag("Blue");

        if (blueTarget == null)
        {
            blueTarget = GameObject.Find("BlueTarget");
        }

        if (blueTarget == null)
        {
            GameObject[] allObjects = FindObjectsOfType<GameObject>();
            foreach (GameObject obj in allObjects)
            {
                if (obj.name.Contains("Blue") && obj.name.Contains("Target"))
                {
                    blueTarget = obj;
                    break;
                }
            }
        }

        if (blueTarget != null)
        {
            Target = blueTarget.transform;
            Debug.Log($"[AI] Blue target assigned: {blueTarget.name} at position {Target.position}");
        }
        else
        {
            Debug.LogWarning("[AI] Blue target not found! Please check target setup.");
        }
    }

    public void ShootTowardsTarget()
    {
        if (Target != null)
        {
            Vector3 direction = (Target.position - transform.position).normalized;

            Debug.Log($"[AI] Shooting towards target at position: {Target.position}");
            Debug.Log($"[AI] Shooting direction: {direction}");
        }
        else
        {
            Debug.LogWarning("[AI] Target not assigned! Cannot shoot towards target.");
        }
    }

    public float GetCurrentDistance()
    {
        if (Bow != null && Target != null)
        {
            float yDiff = Bow.position.y - Target.position.y;
            return Mathf.Abs(yDiff);
        }
        return -1f;
    }

    public bool IsTargetInRange()
    {
        if (Bow != null && Target != null)
        {
            float yDiff = Bow.position.y - Target.position.y;
            float distance = Mathf.Abs(yDiff);

            return distance == 0f || distance == 1f;
        }
        return false;
    }
}

public enum AIMode
{
    Easy,
    Hard
}