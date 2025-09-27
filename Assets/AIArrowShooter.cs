using System.Collections;
using Unity.VisualScripting;
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

    [Header("AI Accuracy Settings")]
    [SerializeField, Range(0f, 1f)] private float easyHitAccuracy = 0.4f; // Lower accuracy for Easy mode
    [SerializeField, Range(0f, 1f)] private float hardHitAccuracy = 0.98f; // Near-perfect accuracy for Hard mode
    [SerializeField] private float easyYAlignTolerance = 1.5f; // Wider tolerance for Easy mode (less accurate)
    [SerializeField] private float hardYAlignTolerance = 1.5f; // Much more forgiving tolerance for Hard mode - allows many more shots
    [SerializeField] private float hardShootForceBonus = 1.2f; // Much faster arrows on hard for better precision
    [SerializeField] private bool requireBelowTarget = true; // Only shoot if bow is at/below target height
    [SerializeField] private float belowApproachOffsetEasy = 0.15f; // Less precise for Easy mode
    [SerializeField] private float belowApproachOffsetHard = 0.08f; // More forgiving for Hard mode
    [SerializeField] private float belowEpsilonEasy = 0.3f; // Wider tolerance for Easy mode
    [SerializeField] private float belowEpsilonHard = 0.8f; // Much more forgiving tolerance for Hard mode

    [Header("Advanced Accuracy Features")]
    [SerializeField] private bool usePredictiveAiming = true; // Predict target movement
    [SerializeField] private float predictionTime = 0.2f; // Longer prediction time for better accuracy
    [SerializeField] private bool useMicroAdjustments = true; // Fine-tune aim based on target velocity
    [SerializeField] private float microAdjustmentStrength = 1.2f; // Much stronger adjustment for Hard mode
    [SerializeField] private bool useAccuracyCurve = true; // Use accuracy curve based on distance
    [SerializeField] private AnimationCurve accuracyCurve = AnimationCurve.Linear(0f, 1f, 10f, 0.8f); // Improved accuracy curve

    [Header("AI Timing")]
    [SerializeField] private float nearCheckInterval = 0.2f;
    [SerializeField] private float nearWindowMultiplier = 2f;
    [SerializeField] private float hardLooseWindow = 1.2f; // Much more forgiving loose window for hard mode
    [SerializeField] private float hardQuickRetry = 0.05f; // Faster retry for hard mode
    [SerializeField] private bool fireImmediatelyOnAlign = true;
    [SerializeField] private float hardImmediateCooldown = 0f; // No delay
    [SerializeField] private float easyImmediateCooldown = 0f; // No delay
    [SerializeField] private float spawnGateDelay = 0f; // No delay
    [SerializeField] private float minGapBetweenShotsHard = 0f; // No delay
    [Header("AI Hold Settings")]
    [SerializeField] private float aiHoldDuration = 0.5f; // No hold delay

    [Header("Random Delay Settings")]
    [SerializeField] private float minRandomDelay = 1f; // Minimum random delay
    [SerializeField] private float maxRandomDelay = 2f; // Maximum random delay

    private Vector3 spawnPosition;
    private float currentDistance = 0f;
    private bool canShoot = false;
    private float nextImmediateTime = 0f;
    private bool isOnCooldown = false;
    private float lastBowY = float.NaN;
    private float lastTargetY = float.NaN;
    private bool isFiring = false;
    private float lastShotTime = -999f;
    private float nextRandomShotTime = 0f; // Time when next shot can be fired

    // Predictive aiming variables
    private Vector3 targetVelocity = Vector3.zero;
    private Vector3 lastTargetPosition = Vector3.zero;
    private float targetVelocityUpdateTime = 0f;

    public bool isGameStart = false;

    public SpriteRenderer BowClickImage;
    public Sprite Check;
    public GameObject BowNoClickImage;
    public GameObject BowNoArrowClickImage;

    [Header("AI Image Switching")]
    public float imageSwitchDuration = 0.5f;

    public GameObject Particals;


    public Animator animator;

    void Start()
    {
        if (shootPoint == null)
        {
            shootPoint = transform;
        }

        if (IFrameBridge.Instance.botLevel == AIMode.Easy)
        {
            currentAIDifficulty = AIMode.Easy;
            shootInterval = 3f; // Slower shooting for Easy mode (less scoring)
        }
        else
        {
            currentAIDifficulty = AIMode.Hard;
            shootInterval = 0.5f; // Very fast shooting for Hard mode to achieve 45-50 score range
        }

        // Assign blue target when AI spawns
        AssignBlueTarget();

        // Initialize last positions for approach detection
        if (Bow != null) lastBowY = Bow.position.y;
        if (Target != null) lastTargetY = Target.position.y;

        StartAiShooting();
    }
    float yDifference;
    float bowY;
    float targetY;
    
    void Update()
    {
        bowY = Bow.position.y;
        targetY = Target.position.y;

         yDifference = Mathf.Abs(bowY - targetY);

        // float bowY1 = Bow.position.y;
        // float targetY1 = Target.position.y;

        // float yDifference1 = Mathf.Abs(bowY1 - targetY1);
        Debug.LogError("Y :" + yDifference);

        //Debug.LogError("Y Difference: " + yDifference);

        if (UIManager.Instance.isGameStart == true)
        {
            spawnPosition = this.gameObject.transform.position;

            isGameStart = UIManager.Instance.isGameStart;

            if (Bow != null && Target != null)
            {
                // Update target velocity for predictive aiming
                UpdateTargetVelocity();

                float yDiff = Bow.position.y - Target.position.y;
                currentDistance = Mathf.Abs(yDiff);

                // Apply accuracy curve based on distance
                float accuracyMultiplier = useAccuracyCurve ? accuracyCurve.Evaluate(currentDistance) : 1f;

                // Difficulty-based alignment window with accuracy multiplier
                float alignTolerance = GetAlignTolerance() * accuracyMultiplier;
                bool isAligned = currentDistance <= alignTolerance;
                bool hardCanForce = currentAIDifficulty == AIMode.Hard && currentDistance <= hardLooseWindow;

                // Detect converging motion: bow and target moving towards each other along Y
                float bowDelta = float.IsNaN(lastBowY) ? 0f : Bow.position.y - lastBowY;
                float targetDelta = float.IsNaN(lastTargetY) ? 0f : Target.position.y - lastTargetY;

                // Bow coming down (negative delta) and target going up (positive delta) = converging
                bool bowDownTargetUp = (bowDelta < 0f && targetDelta > 0f);
                // Bow going up (positive delta) and target coming down (negative delta) = converging  
                bool bowUpTargetDown = (bowDelta > 0f && targetDelta < 0f);
                bool converging = bowDownTargetUp || bowUpTargetDown;

                // Shoot when converging and within range (Hard mode difference = 3)
                float convergingRange = currentAIDifficulty == AIMode.Hard ? 1f : 2f; // Hard mode difference increased to 3
                bool convergingWithinRange = converging && currentDistance <= convergingRange;

                // Shoot when Y positions are close (more forgiving for Hard mode)
                float exactMatchThreshold = currentAIDifficulty == AIMode.Hard ? 0.3f : 0.1f;
                bool exactMatch = Mathf.Abs(currentDistance) <= exactMatchThreshold;

                // Also allow shot when integer Y band matches (first digit same)
                bool sameYBand = Mathf.FloorToInt(Bow.position.y) == Mathf.FloorToInt(Target.position.y);

                // Enhanced alignment check with predictive aiming
                bool predictiveAligned = false;
                if (usePredictiveAiming && currentAIDifficulty == AIMode.Hard)
                {
                    Vector3 predictedTargetPos = Target.position + (targetVelocity * predictionTime);
                    float predictedYDiff = Bow.position.y - predictedTargetPos.y;
                    predictiveAligned = Mathf.Abs(predictedYDiff) <= alignTolerance * 1.2f; // More forgiving for Hard mode
                }

                // Also enable predictive aiming for Easy mode with reduced effectiveness
                bool easyPredictiveAligned = false;
                if (usePredictiveAiming && currentAIDifficulty == AIMode.Easy)
                {
                    Vector3 predictedTargetPos = Target.position + (targetVelocity * predictionTime * 0.5f);
                    float predictedYDiff = Bow.position.y - predictedTargetPos.y;
                    easyPredictiveAligned = Mathf.Abs(predictedYDiff) <= alignTolerance * 0.9f;
                }

                // Check Y distance first - don't shoot if too far
                if (yDifference >= 3f)
                {
                    canShoot = false;
                    Debug.Log($"AI not shooting - Y distance too large: {yDifference:F2}");
                }
                // New shooting logic based on converging motion
                else if (exactMatch)
                {
                    // Always shoot when Y positions are exactly the same
                    canShoot = true;
                    Debug.Log("Exact Y position match - shooting!");
                }
                else if (convergingWithinRange)
                {
                    // Shoot when converging and within 2 units
                    canShoot = true;
                    Debug.Log($"Converging motion detected - shooting! Distance: {currentDistance:F2}");
                }
                else if (currentAIDifficulty == AIMode.Hard)
                {
                    // For Hard: Very aggressive approach - shoot almost always to maximize hits
                    // Use very forgiving conditions to ensure high hit rate
                    canShoot = (currentDistance <= hardYAlignTolerance) || 
                               (Mathf.Abs(yDifference) <= 1.0f) || 
                               sameYBand || 
                               predictiveAligned || 
                               isAligned ||
                               hardCanForce ||
                               convergingWithinRange;
                    Debug.Log($"Hard mode - Y distance: {yDifference:F2}, Current distance: {currentDistance:F2}, Can shoot: {canShoot}");
                }
                else if (requireBelowTarget)
                {
                    // Shoot only when bow is level with or below target (arrow meets target from below)
                    float eps = currentAIDifficulty == AIMode.Hard ? belowEpsilonHard : belowEpsilonEasy;
                    bool bowBelowOrEqual = Bow.position.y <= Target.position.y + eps;
                    canShoot = (isAligned || hardCanForce || predictiveAligned || easyPredictiveAligned) && bowBelowOrEqual;
                }
                else
                {
                    canShoot = isAligned || hardCanForce || predictiveAligned || easyPredictiveAligned;
                }

                // Update last positions after logic
                lastBowY = Bow.position.y;
                lastTargetY = Target.position.y;
            }
            else
            {
                canShoot = false;
            }

            // Fire with random delay between shots
            if (aiAutoShoot && fireImmediatelyOnAlign && canShoot && !isFiring && Time.time >= nextRandomShotTime)
            {

                animator.SetBool("isClick", true); // Trigger animation when shooting logic is met
                Invoke(nameof(LateCallSound), 0.3f);
                StartCoroutine(ShootArrow());
            }
        }
    }

    public void LateCallSound()
    {
        SoundManager.Instance.PlayRandomBowPull();

    }

    void StartAiShooting()
    {
        Invoke(nameof(AIShootCycle), shootInterval);
    }

    void AIShootCycle()
    {
        if (!aiAutoShoot) return;

        if (canShoot && Time.time >= nextRandomShotTime)
        {
            Debug.Log($"{currentAIDifficulty} AI - Distance {currentDistance:F2} <= {maxShootDistance}. Shooting now!");
            animator.SetBool("isClick", true); // Trigger animation when shooting logic is met
            // if (SoundManager.Instance != null)
            // {
            //     SoundManager.Instance.PlayRandomBowPull();
            // }
            StartCoroutine(ShootArrow());
            Invoke(nameof(AIShootCycle), shootInterval);
            return;
        }
        else
        {
            Debug.Log($"{currentAIDifficulty} AI - Distance {currentDistance:F2} > align. Waiting...");
            // If we're near alignment, check again sooner to reduce perceived waiting
            float alignTolerance = GetAlignTolerance();
            float quick = currentAIDifficulty == AIMode.Hard ? hardQuickRetry : nearCheckInterval;
            float nextDelay = currentDistance <= alignTolerance * nearWindowMultiplier ? quick : shootInterval;
            //Invoke(nameof(AIShootCycle), nextDelay);
            return;
        }
    }

    float GetAlignTolerance()
    {
        return currentAIDifficulty == AIMode.Hard ? hardYAlignTolerance : easyYAlignTolerance;
    }
    public IEnumerator ShootArrow()
    {

        if (UIManager.Instance.isGameStart == false)
        {
            StopCoroutine(ShootArrow());
            yield break;
        }

        if (isFiring)
            yield break;
        // Removed cooldown check for immediate shooting
        isFiring = true;

        // BowClickImage.SetActive(true);
        // BowNoClickImage.SetActive(false);

        // Animation is now triggered at the shooting logic location

        if (arrowPrefab == null)
        {
            Debug.LogWarning("AI Arrow Prefab is not assigned!");
            yield break;
        }

        // Play stretch sound immediately when stretch animation starts


        // Wait for stretch animation to complete
        yield return new WaitForSeconds(aiHoldDuration);

        Vector3 adjustedSpawn = shootPoint != null ? shootPoint.position : spawnPosition;
        Vector3 microAdjustment = CalculateMicroAdjustment();
        adjustedSpawn += microAdjustment;

        GameObject newArrow = null;

        // Create arrow (condition already checked at start of method)
        newArrow = Instantiate(arrowPrefab, adjustedSpawn, Quaternion.identity);
        Debug.Log($"AI shooting - Target above bow: TargetY={targetY}, BowY={bowY}, Difference={yDifference}");

        //animator.SetBool("isGo", true);

        //BowClickImage.SetActive(false);

        BowClickImage.sprite = Check;
        BowNoClickImage.SetActive(false);
        BowNoArrowClickImage.SetActive(true);


        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayRandomBowRelease();
        }
        lastShotTime = Time.time;

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
                float finalForce = shootForce * (currentAIDifficulty == AIMode.Hard ? hardShootForceBonus : 1f);
                arrowRb.linearVelocity = forceDirection * finalForce;
                Debug.Log("AI Direct velocity set: " + arrowRb.linearVelocity);
            }
            else
            {
                float finalForce = shootForce * (currentAIDifficulty == AIMode.Hard ? hardShootForceBonus : 1f);
                arrowRb.AddForce(forceDirection * finalForce, ForceMode2D.Impulse);
                Debug.Log("AI Force applied: " + (forceDirection * finalForce));
            }

            Debug.Log("AI Arrow spawned at: " + spawnPosition + " with force: " + shootForce);

            // Disable AI target speed control for maximum hit rate
            if (Target != null)
            {
                AITargetSpeedController aiTargetController = Target.GetComponent<AITargetSpeedController>();
                if (aiTargetController != null)
                {
                    // Completely disable target speed increases to maximize hit rate
                    // Only occasionally move target to arrow spawn position
                    if (Random.Range(0f, 1f) < 0.2f) // 20% chance to move target
                    {
                        aiTargetController.MoveToArrowSpawnPosition(adjustedSpawn);
                        Debug.Log($"AI triggered target to move to arrow spawn position: {adjustedSpawn}");
                    }
                }
            }
        }
        else
        {
            Debug.LogWarning("AI Arrow prefab doesn't have Rigidbody2D component!");
        }

        // Removed all delays for immediate shooting
        animator.SetBool("isClick", false);
        BowNoArrowClickImage.SetActive(false);
        BowNoClickImage.SetActive(true);

        isFiring = false;

        // Set next random shot time (different delays for Easy vs Hard)
        float randomDelay;
        if (currentAIDifficulty == AIMode.Easy)
        {
            // Easy mode: longer delays (2-4 seconds) = less scoring
            randomDelay = Random.Range(2f, 3f);
        }
        else
        {
            // Hard mode: very short delay to maximize shooting frequency
            // This will help achieve the 45-50 score range by shooting very frequently
            randomDelay = Random.Range(0.3f, 0.6f);
        }
        nextRandomShotTime = Time.time + randomDelay;
        Debug.Log($"{currentAIDifficulty} mode - Next shot in {randomDelay:F2} seconds");

        yield return new WaitForSeconds(arrowLifetime);

        if (newArrow != null)
        {
            Destroy(newArrow);
        }

    }

    public void NotifyAIArrowHit()
    {
        // Removed cooldown logic for immediate shooting
    }

    private IEnumerator CooldownAfterHit()
    {
        // Removed cooldown logic for immediate shooting
        yield break;
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
            GameObject[] allObjects = FindObjectsByType<GameObject>(FindObjectsSortMode.None);
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

    public void ParticalFalse()
    {
        Particals.GetComponent<ParticleSystem>().Stop();
        Particals.SetActive(false);
    }

    private void UpdateTargetVelocity()
    {
        if (Time.time - targetVelocityUpdateTime > 0.1f) // Update every 0.1 seconds
        {
            if (lastTargetPosition != Vector3.zero)
            {
                targetVelocity = (Target.position - lastTargetPosition) / (Time.time - targetVelocityUpdateTime);
            }
            lastTargetPosition = Target.position;
            targetVelocityUpdateTime = Time.time;
        }
    }

    private Vector3 CalculateMicroAdjustment()
    {
        if (!useMicroAdjustments)
            return Vector3.zero;

        Vector3 adjustment = Vector3.zero;
        if (targetVelocity.magnitude > 0.05f) // Lower threshold for more responsive adjustments
        {
            float yDiff = Bow.position.y - Target.position.y;
            if (Mathf.Abs(yDiff) > 0.05f) // Lower threshold for more adjustments
            {
                float adjustmentStrength = currentAIDifficulty == AIMode.Hard ? microAdjustmentStrength : microAdjustmentStrength * 0.6f;
                adjustment.y = -targetVelocity.y * adjustmentStrength * 0.15f; // Increased adjustment factor
            }
        }
        return adjustment;
    }
}

public enum AIMode
{
    Easy,
    Hard
}