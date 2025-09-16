using System.Collections;
using UnityEngine;

public class AIArrowShooter : MonoBehaviour
{
    [Header("AI Arrow Settings")]
    public GameObject arrowPrefab;         // તીરનો prefab
    public Transform shootPoint;          // તીર ક્યાંથી શૂટ થશે
    public float shootForce = 10f;        // તીર પર લગાવવાનું બળ
    public bool isLeft = false;           // AI માત્ર જમણી બાજુથી ડાબી બાજુ શૂટ કરે

    [Header("AI Shooting Controls")]
    public float shootInterval = 1f;     // AI કેટલા સમયમાં શૂટ કરે (1 second)
    public bool aiAutoShoot = true;       // AI automatically શૂટ કરે કે નહીં
    public float aiReactionTime = 0.5f;   // AI નો reaction time (ઓછું)

    [Header("Arrow Physics")]
    public bool useGravity = false;       // gravity લાગવી છે કે નહીં
    public float arrowLifetime = 3f;     // Arrow lifetime ઓછું (3 seconds)
    public bool resetVelocityOnSpawn = true;
    public bool useDirectVelocity = true;

    [Header("AI Difficulty")]
    public float aiAccuracy = 0.8f;       // AI ની accuracy
    
    [Header("AI Difficulty Levels")]
    public AIDifficultyLevel currentAIDifficulty = AIDifficultyLevel.Easy;
    
    public enum AIDifficultyLevel
    {
        Easy,    // 40% win chance
        Hard     // 60% win chance
    }

    private Vector3 spawnPosition;
    private bool canShoot = true;

    void Start()
    {
        // જો shootPoint set નથી કર્યું તો bow ની position ઉપયોગ કરો
        if (shootPoint == null)
        {
            shootPoint = transform;
        }

        // AI difficulty settings apply કરો
        ApplyDifficultySettings();
        
        // Game start થાય તો AI automatically શૂટ કરવાનું શરૂ કરો
        Debug.Log("Game Started - AI will start shooting arrows!");
        Debug.Log("AI Difficulty: " + currentAIDifficulty);
        
        // Single Invoke use કરો shooting માટે
        StartRandomShooting();
    }

    void Update()
    {
        spawnPosition = this.gameObject.transform.position;
    }

    // Single Invoke use કરીને shooting શરૂ કરવા માટે
    void StartRandomShooting()
    {
        // 1 second wait કરો game start થાય તો
        Invoke(nameof(AIShootCycle), 1f);
    }
    
    // AI shooting cycle - માત્ર એક Invoke
    void AIShootCycle()
    {
        if (!aiAutoShoot) return;
        
        Debug.Log("AI Shooting Arrow!");
        
        // AI accuracy check કરો
        if (CheckAIAccuracy())
        {
            StartCoroutine(ShootArrow());
        }
        else
        {
            Debug.Log("AI missed the shot!");
        }
        
        // Next shot schedule કરો (માત્ર એક Invoke)
        Invoke(nameof(AIShootCycle), shootInterval);
    }


    // AI difficulty settings apply કરવા માટે
    void ApplyDifficultySettings()
    {
        if (currentAIDifficulty == AIDifficultyLevel.Easy)
        {
            // Easy Mode: 40% win chance
            aiAccuracy = 0.4f;           // 40% accuracy
            Debug.Log("AI Easy Mode Applied - 40% win chance");
        }
        else if (currentAIDifficulty == AIDifficultyLevel.Hard)
        {
            // Hard Mode: 60% win chance
            aiAccuracy = 0.6f;           // 60% accuracy
            Debug.Log("AI Hard Mode Applied - 60% win chance");
        }
    }
    
    // AI ની accuracy check કરવા માટે
    bool CheckAIAccuracy()
    {
        float randomValue = Random.Range(0f, 1f);
        bool hit = randomValue <= aiAccuracy;
        
        Debug.Log("AI Accuracy Check: " + (hit ? "HIT" : "MISS") + " (Random: " + randomValue + " <= Accuracy: " + aiAccuracy + ")");
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
