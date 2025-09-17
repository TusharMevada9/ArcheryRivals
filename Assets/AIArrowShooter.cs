using System.Collections;
using UnityEngine;

public class AIArrowShooter : MonoBehaviour
{
    [Header("AI Arrow Settings")]
    public GameObject arrowPrefab;         // તીરનો prefab
    public Transform shootPoint;          // તીર ક્યાંથી શૂટ થશે
    public float shootForce = 8f;        // તીર પર લગાવવાનું બળ (ઓછું બળ - ધીમું)
    public bool isLeft = false;           // AI માત્ર જમણી બાજુથી ડાબી બાજુ શૂટ કરે

    [Header("AI Shooting Controls")]
    public float shootInterval = 1f;     // AI કેટલા સમયમાં શૂટ કરે (2 seconds - ધીમું)
    public bool aiAutoShoot = true;       // AI automatically શૂટ કરે કે નહીં
    public float aiReactionTime = 0.5f;   // AI નો reaction time (ઓછું)

    [Header("Arrow Physics")]
    public bool useGravity = false;       // gravity લાગવી છે કે નહીં
    public float arrowLifetime = 3f;     // Arrow lifetime ઓછું (3 seconds)
    public bool resetVelocityOnSpawn = true;
    public bool useDirectVelocity = true;

    [Header("AI Difficulty")]
    public float aiAccuracy = 0.8f;       // AI ની accuracy
    
    [Header("Line Detection")]
    public Transform playerTransform;     // Player નો transform
    public Transform targetTransform;     // Target circle નો transform
    public float lineDetectionRange = 0.5f; // Line detection ની range (ઓછી range - exact match)
    public float maxWaitTime = 2f;        // Maximum wait time before shooting anyway

    public float shootWaitTime = 0.5f;
    
    [Header("AI Difficulty Levels")]
    public AIDifficultyLevel currentAIDifficulty = AIDifficultyLevel.Easy;
    
    public enum AIDifficultyLevel
    {
        Easy,    // 40% win chance
        Hard     // 60% win chance
    }

    private Vector3 spawnPosition;
    private bool canShoot = true;
    private float waitTimer = 0f;         // Wait timer

    void Start()
    {
        if (shootPoint == null)
        {
            shootPoint = transform;
        }

        // AI difficulty settings apply કરો
        ApplyDifficultySettings();

        // Game start થાય તો AI automatically શૂટ કરવાનું શરૂ કરો
        Debug.Log("Game Started - AI will start shooting arrows!");
        Debug.Log("AI Difficulty: " + currentAIDifficulty);

        StartRandomShooting();

        if (currentAIDifficulty == AIDifficultyLevel.Easy)
        {
            shootWaitTime = 1f;
        }
        else
        {
             shootWaitTime = 0.5f;
        }
    }

    void Update()
    {
        spawnPosition = this.gameObject.transform.position;
    }

    void StartRandomShooting()
    {
        // 1 second wait કરો game start થાય તો
        Invoke(nameof(AIShootCycle), 1f);
    }
    
    void AIShootCycle()
    {
        if (!aiAutoShoot) return;
        
        // Check if player and target Y positions are close (both Easy and Hard AI)
        if (ArePositionsClose())
        {
            Debug.Log($"{currentAIDifficulty} AI - Player and Target Y positions close - AI will shoot in 1 second!");
            waitTimer = 0f; // Reset wait timer


            float Temp = Random.Range(0, 1);
            // Wait 1 second then shoot
            Invoke(nameof(DelayedShoot), Temp);
        }
        else
        {
            waitTimer += shootInterval; // Add to wait timer
            Debug.Log($"{currentAIDifficulty} AI - Player and Target Y positions not close - AI waiting... ({waitTimer}/{maxWaitTime})");
            
            // If waited too long, shoot anyway
            if (waitTimer >= maxWaitTime)
            {
                Debug.Log("AI waited too long - shooting anyway!");
                waitTimer = 0f; // Reset timer
                
                // Direct shoot without accuracy check
                StartCoroutine(ShootArrow());
            }
        }
        
        Invoke(nameof(AIShootCycle), shootInterval);
    }


    // AI difficulty settings apply કરવા માટે
    void ApplyDifficultySettings()
    {
        if (currentAIDifficulty == AIDifficultyLevel.Easy)
        {
            aiAccuracy = 0.5f;           // 50% accuracy - balanced
            shootInterval = 1f;          // હંમેશા 2 seconds - accuracy ના કારણે બદલાશે નહીં
        }
        else if (currentAIDifficulty == AIDifficultyLevel.Hard)
        {
            aiAccuracy = 0.6f;           // 60% accuracy - થોડું વધારે પણ ખૂબ વધારે નહીં
            shootInterval = 1f;          // હંમેશા 2 seconds - accuracy ના કારણે બદલાશે નહીં
        }
    }
    
    // Check if player and target Y positions are close (for both Easy and Hard AI)
    bool ArePositionsClose()
    {
        if (playerTransform == null || targetTransform == null)
        {
            Debug.LogWarning("Player or Target Transform not assigned!");
            return true; // If not assigned, shoot anyway
        }
        
        // Get Y positions
        float playerY = playerTransform.position.y;
        float targetY = targetTransform.position.y;
        
        // Calculate Y position difference
        float yDifference = Mathf.Abs(playerY - targetY);
        
        // If Y difference is within range, they are close
        bool positionsClose = yDifference <= lineDetectionRange;
        
        Debug.Log($"Player Y: {playerY}, Target Y: {targetY}, Difference: {yDifference}, Positions Close: {positionsClose}");
        
        return positionsClose;
    }
    
    // Delayed shoot function (called after 1 second delay)
    void DelayedShoot()
    {
        Debug.Log($"{currentAIDifficulty} AI - 1 second delay completed - AI shooting now!");
        StartCoroutine(ShootArrow());
    }
    
    // AI ની accuracy check કરવા માટે
    bool CheckAIAccuracy()
    {
        float randomValue = Random.Range(0f, 1f);
        bool hit = randomValue <= aiAccuracy;
        
        return hit;
    }

    // તીર શૂટ કરવાનું ફંક્શન
    public IEnumerator ShootArrow()
    {
        if (arrowPrefab == null)
        {
            Debug.LogWarning("AI Arrow Prefab is not assigned!");
            yield break;
        }

        canShoot = false;

        yield return new WaitForSeconds(shootWaitTime);

        // તીર instantiate કરો
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

        canShoot = true;
        Debug.Log("AI Arrow Shot! Force: " + shootForce);
    }

   
    public void SetAIDifficulty(AIDifficultyLevel difficulty)
    {
        currentAIDifficulty = difficulty;
        ApplyDifficultySettings();
        Debug.Log("AI Difficulty changed to: " + currentAIDifficulty);
    }

}
