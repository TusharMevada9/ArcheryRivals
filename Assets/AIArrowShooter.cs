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
        spawnPosition = this.gameObject.transform.position;
        
        isGameStart = UIManager.Instance.isGameStart;

        // Calculate Y difference with 3 conditions
        if (Bow != null && Target != null)
        {
            float yDiff = Bow.position.y - Target.position.y;
            currentDistance = Mathf.Abs(yDiff); // Use absolute difference

            // 3 Conditions for shooting:
            // 1. Difference = 0 → Shoot
            // 2. Difference = 1 → Shoot  
            // 3. Difference > 1 → Don't shoot
            if (currentDistance >= 0f && currentDistance <= 1f)
            {
                canShoot = true; // Condition 1: Same level
                Debug.LogError("Check 1");

            }
            else if (currentDistance >= 1f && currentDistance <= 4f)
            {
                canShoot = true; // Condition 2: 1 unit difference
                Debug.LogError("Check 2");
            }
            else
            {
                canShoot = false; // Condition 3: More than 1 unit difference
            }
            
            // Debug log every few frames to avoid spam
            if (Time.frameCount % 60 == 0) // Log every 60 frames (about 1 second at 60fps)
            {
                Debug.Log($"[AI] Update - BowY: {Bow.position.y:F2}, TargetY: {Target.position.y:F2}");
                Debug.Log($"[AI] Update - YDiff: {yDiff:F2}, |YDiff|: {currentDistance:F2}");
                Debug.Log($"[AI] Update - Condition: {(currentDistance == 0f ? "Same Level" : currentDistance == 1f ? "1 Unit Diff" : "Too Far")}, CanShoot: {canShoot}");
            }
        }
        else
        {
            canShoot = false;
        }
    }

    void StartAiShooting()
    {
        // 2 second wait કરો game start થાય તો
        Invoke(nameof(AIShootCycle), shootInterval);
    }
    
    void AIShootCycle()
    {
        if (!aiAutoShoot) return;

        // Use the canShoot variable calculated in Update method
        if (canShoot)
        {
            Debug.Log($"{currentAIDifficulty} AI - Distance {currentDistance:F2} <= {maxShootDistance}. Shooting now!");
            StartCoroutine(ShootArrow());
        }
        else
        {
            Debug.Log($"{currentAIDifficulty} AI - Distance {currentDistance:F2} > {maxShootDistance}. Waiting...");
        }

        // Schedule next check
        Invoke(nameof(AIShootCycle), shootInterval);
    }
    public IEnumerator ShootArrow()
    {
        if (arrowPrefab == null)
        {
            Debug.LogWarning("AI Arrow Prefab is not assigned!");
            yield break;
        }
        GameObject newArrow = Instantiate(arrowPrefab, spawnPosition, Quaternion.identity);

        // Rigidbody2D મળે તો force લગાવો
        Rigidbody2D arrowRb = newArrow.GetComponent<Rigidbody2D>();
        if (arrowRb != null)
        {
            // પહેલા velocity zero કરો
            if (resetVelocityOnSpawn)
            {
                arrowRb.linearVelocity = Vector2.zero;
                arrowRb.angularVelocity = 0f;
            }

            // AI માત્ર જમણી બાજુથી ડાબી બાજુ શૂટ કરે
            Vector2 forceDirection = new Vector2(-1f, 0f); // ડાબી બાજુ તરફ

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

        // AI shot successful
        Debug.Log("AI Arrow shot successfully!");

        yield return new WaitForSeconds(arrowLifetime);

        if (newArrow != null)
        {
            Destroy(newArrow);
        }

    }

    // Method to find and assign blue target
    public void AssignBlueTarget()
    {
        // Find blue target by tag or name
        GameObject blueTarget = GameObject.FindGameObjectWithTag("Blue");
        
        if (blueTarget == null)
        {
            // Try finding by name
            blueTarget = GameObject.Find("BlueTarget");
        }
        
        if (blueTarget == null)
        {
            // Try finding any object with "Blue" in the name
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

    // Method to shoot towards the assigned target
    public void ShootTowardsTarget()
    {
        if (Target != null)
        {
            // Calculate direction from AI to target
            Vector3 direction = (Target.position - transform.position).normalized;
            
            // Use this direction for shooting
            Debug.Log($"[AI] Shooting towards target at position: {Target.position}");
            Debug.Log($"[AI] Shooting direction: {direction}");
        }
        else
        {
            Debug.LogWarning("[AI] Target not assigned! Cannot shoot towards target.");
        }
    }

    // Method to get current distance using 3-condition logic
    public float GetCurrentDistance()
    {
        if (Bow != null && Target != null)
        {
            float yDiff = Bow.position.y - Target.position.y;
            return Mathf.Abs(yDiff);
        }
        return -1f; // Invalid distance
    }

    // Method to check if target is within range using 3-condition logic
    public bool IsTargetInRange()
    {
        if (Bow != null && Target != null)
        {
            float yDiff = Bow.position.y - Target.position.y;
            float distance = Mathf.Abs(yDiff);
            
            // 3 Conditions: 0, 1, or >1
            return distance == 0f || distance == 1f;
        }
        return false;
    }
}

public enum AIMode
{
    Easy,    // 40% win chance
    Hard     // 60% win chance
}