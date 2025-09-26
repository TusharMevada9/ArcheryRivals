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

    [Header("AI Accuracy Settings")]
    [SerializeField, Range(0f, 1f)] private float easyHitAccuracy = 0.85f; // Increased probability to align for a hit
    [SerializeField, Range(0f, 1f)] private float hardHitAccuracy = 0.99f; // Higher probability on hard
    [SerializeField] private float easyYAlignTolerance = 0.6f; // Tighter Y diff window to shoot
    [SerializeField] private float hardYAlignTolerance = 0.15f; // Much tighter tolerance for better accuracy
    [SerializeField] private float hardShootForceBonus = 1.2f; // Faster arrows on hard for better precision
    [SerializeField] private bool requireBelowTarget = true; // Only shoot if bow is at/below target height
    [SerializeField] private float belowApproachOffsetEasy = 0.05f; // More precise spawn positioning
    [SerializeField] private float belowApproachOffsetHard = 0.01f; // Very precise positioning
    [SerializeField] private float belowEpsilonEasy = 0.08f; // Tighter tolerance above target
    [SerializeField] private float belowEpsilonHard = 0.005f; // Extremely tight tolerance for hard mode
    
    [Header("Advanced Accuracy Features")]
    [SerializeField] private bool usePredictiveAiming = true; // Predict target movement
    [SerializeField] private float predictionTime = 0.15f; // Increased prediction time for better accuracy
    [SerializeField] private bool useMicroAdjustments = true; // Fine-tune aim based on target velocity
    [SerializeField] private float microAdjustmentStrength = 0.8f; // Increased adjustment strength
    [SerializeField] private bool useAccuracyCurve = true; // Use accuracy curve based on distance
    [SerializeField] private AnimationCurve accuracyCurve = AnimationCurve.Linear(0f, 1f, 10f, 0.8f); // Improved accuracy curve

    [Header("AI Timing")]
    [SerializeField] private float nearCheckInterval = 0.2f;
    [SerializeField] private float nearWindowMultiplier = 2f;
    [SerializeField] private float hardLooseWindow = 0.5f; // Tighter loose window for hard mode
    [SerializeField] private float hardQuickRetry = 0.05f; // Faster retry for hard mode
    [SerializeField] private bool fireImmediatelyOnAlign = true;
    [SerializeField] private float hardImmediateCooldown = 0.5f;
    [SerializeField] private float easyImmediateCooldown = 0.5f;
    [SerializeField] private float spawnGateDelay = 0.1f;
    [SerializeField] private float minGapBetweenShotsHard = 0.5f;
    [Header("AI Hold Settings")]
    [SerializeField] private float aiHoldDuration = 2.0f;

    private Vector3 spawnPosition;
    private float currentDistance = 0f;
    private bool canShoot = false;
    private float nextImmediateTime = 0f;
    private bool isOnCooldown = false;
    private float lastBowY = float.NaN;
    private float lastTargetY = float.NaN;
    private bool isFiring = false;
    private float lastShotTime = -999f;
    
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
            shootInterval = 2f;
        }
        else
        {
            currentAIDifficulty = AIMode.Hard;
            shootInterval = 1.2f;
        }

        // Assign blue target when AI spawns
        AssignBlueTarget();

        // Initialize last positions for approach detection
        if (Bow != null) lastBowY = Bow.position.y;
        if (Target != null) lastTargetY = Target.position.y;

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

                // Detect approaching motion: bow and target moving towards each other along Y
                float bowDelta = float.IsNaN(lastBowY) ? 0f : Bow.position.y - lastBowY;
                float targetDelta = float.IsNaN(lastTargetY) ? 0f : Target.position.y - lastTargetY;
                bool approaching = (bowDelta < 0f && targetDelta > 0f) || (bowDelta > 0f && targetDelta < 0f);
                // Also allow shot when integer Y band matches (first digit same)
                bool sameYBand = Mathf.FloorToInt(Bow.position.y) == Mathf.FloorToInt(Target.position.y);
                
                // Enhanced alignment check with predictive aiming
                bool predictiveAligned = false;
                if (usePredictiveAiming && currentAIDifficulty == AIMode.Hard)
                {
                    Vector3 predictedTargetPos = Target.position + (targetVelocity * predictionTime);
                    float predictedYDiff = Bow.position.y - predictedTargetPos.y;
                    predictiveAligned = Mathf.Abs(predictedYDiff) <= alignTolerance * 0.6f; // Even tighter for prediction
                }
                
                // Also enable predictive aiming for Easy mode with reduced effectiveness
                bool easyPredictiveAligned = false;
                if (usePredictiveAiming && currentAIDifficulty == AIMode.Easy)
                {
                    Vector3 predictedTargetPos = Target.position + (targetVelocity * predictionTime * 0.5f);
                    float predictedYDiff = Bow.position.y - predictedTargetPos.y;
                    easyPredictiveAligned = Mathf.Abs(predictedYDiff) <= alignTolerance * 0.9f;
                }

                if (currentAIDifficulty == AIMode.Hard)
                {
                    // For Hard: fire when approaching and within ~1 unit OR when in same integer Y band OR predictive alignment
                    canShoot = (approaching && currentDistance <= 1f) || sameYBand || predictiveAligned;
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

            // Immediate fire to avoid missing brief overlaps
            if (aiAutoShoot && fireImmediatelyOnAlign && canShoot && Time.time >= nextImmediateTime && !isOnCooldown && !isFiring)
            {
                // Only rate-limit immediate shots when already cooling down
                if (isOnCooldown)
                    nextImmediateTime = Time.time + (currentAIDifficulty == AIMode.Hard ? hardImmediateCooldown : easyImmediateCooldown);
                StartCoroutine(ShootArrow());
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

        if (canShoot && !isOnCooldown)
        {
            Debug.Log($"{currentAIDifficulty} AI - Distance {currentDistance:F2} <= {maxShootDistance}. Shooting now!");
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
            Invoke(nameof(AIShootCycle), nextDelay);
            return;
        }
    }

    float GetAlignTolerance()
    {
        return currentAIDifficulty == AIMode.Hard ? hardYAlignTolerance : easyYAlignTolerance;
    }
    public IEnumerator ShootArrow()
    {
        if (isFiring)
            yield break;
        if (currentAIDifficulty == AIMode.Hard && Time.time < lastShotTime + minGapBetweenShotsHard)
            yield break;
        isFiring = true;

        // BowClickImage.SetActive(true);
        // BowNoClickImage.SetActive(false);

        animator.SetBool("isClick", true);

        if (arrowPrefab == null)
        {
            Debug.LogWarning("AI Arrow Prefab is not assigned!");
            yield break;
        }

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayRandomBowPull();
        }
        yield return new WaitForSeconds(aiHoldDuration);

        Vector3 adjustedSpawn = shootPoint != null ? shootPoint.position : spawnPosition;
        Vector3 microAdjustment = CalculateMicroAdjustment();
        adjustedSpawn += microAdjustment;


        GameObject newArrow = Instantiate(arrowPrefab, adjustedSpawn, Quaternion.identity);

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
        }
        else
        {
            Debug.LogWarning("AI Arrow prefab doesn't have Rigidbody2D component!");
        }

        yield return new WaitForSeconds(1f);

        animator.SetBool("isClick", false);
        BowNoArrowClickImage.SetActive(false);
        BowNoClickImage.SetActive(true);

        yield return new WaitForSeconds(spawnGateDelay);
        isFiring = false;

        yield return new WaitForSeconds(arrowLifetime);

        if (newArrow != null)
        {
            Destroy(newArrow);
        }

    }

    public void NotifyAIArrowHit()
    {
        if (!isOnCooldown)
        {
            StartCoroutine(CooldownAfterHit());
        }
    }

    private IEnumerator CooldownAfterHit()
    {
        isOnCooldown = true;
        float cd = currentAIDifficulty == AIMode.Hard ? hardImmediateCooldown : easyImmediateCooldown;
        yield return new WaitForSeconds(cd);
        isOnCooldown = false;
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