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
    [SerializeField, Range(0f, 1f)] private float easyHitAccuracy = 0.65f; // Probability to align for a hit
    [SerializeField, Range(0f, 1f)] private float hardHitAccuracy = 0.93f; // Higher probability on hard
    [SerializeField] private float easyYAlignTolerance = 1.0f; // Acceptable Y diff window to shoot
    [SerializeField] private float hardYAlignTolerance = 0.5f; // Wider so hard shoots more often
    [SerializeField] private float hardShootForceBonus = 1.15f; // Slightly faster arrows on hard
    [SerializeField] private bool requireBelowTarget = true; // Only shoot if bow is at/below target height
    [SerializeField] private float belowApproachOffsetEasy = 0.08f; // Spawn slightly below target so arrow comes up into it
    [SerializeField] private float belowApproachOffsetHard = 0.03f;
    [SerializeField] private float belowEpsilonEasy = 0.15f; // Allow slight tolerance above target
    [SerializeField] private float belowEpsilonHard = 0.02f;

    [Header("AI Timing")]
    [SerializeField] private float nearCheckInterval = 0.2f;
    [SerializeField] private float nearWindowMultiplier = 2f;
    [SerializeField] private float hardLooseWindow = 1.2f;
    [SerializeField] private float hardQuickRetry = 0.08f;
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

    public bool isGameStart = false;

    public GameObject BowClickImage;
    public GameObject BowNoClickImage;
    public GameObject BowNoArrowClickImage;

    [Header("AI Image Switching")]
    public float imageSwitchDuration = 0.5f;

    public GameObject Particals;

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
                float yDiff = Bow.position.y - Target.position.y;
                currentDistance = Mathf.Abs(yDiff);

                // Difficulty-based alignment window
                float alignTolerance = GetAlignTolerance();
                bool isAligned = currentDistance <= alignTolerance;
                bool hardCanForce = currentAIDifficulty == AIMode.Hard && currentDistance <= hardLooseWindow;

                // Detect approaching motion: bow and target moving towards each other along Y
                float bowDelta = float.IsNaN(lastBowY) ? 0f : Bow.position.y - lastBowY;
                float targetDelta = float.IsNaN(lastTargetY) ? 0f : Target.position.y - lastTargetY;
                bool approaching = (bowDelta < 0f && targetDelta > 0f) || (bowDelta > 0f && targetDelta < 0f);
                // Also allow shot when integer Y band matches (first digit same)
                bool sameYBand = Mathf.FloorToInt(Bow.position.y) == Mathf.FloorToInt(Target.position.y);

                if (currentAIDifficulty == AIMode.Hard)
                {
                    // For Hard: fire when approaching and within ~1 unit OR when in same integer Y band
                    canShoot = (approaching && currentDistance <= 1f) || sameYBand;
                }
                else if (requireBelowTarget)
                {
                    // Shoot only when bow is level with or below target (arrow meets target from below)
                    float eps = currentAIDifficulty == AIMode.Hard ? belowEpsilonHard : belowEpsilonEasy;
                    bool bowBelowOrEqual = Bow.position.y <= Target.position.y + eps;
                    canShoot = (isAligned || hardCanForce) && bowBelowOrEqual;
                }
                else
                {
                    canShoot = isAligned || hardCanForce;
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
        // Enforce hard-mode global shot gap regardless of hit
        if (currentAIDifficulty == AIMode.Hard && Time.time < lastShotTime + minGapBetweenShotsHard)
            yield break;
        isFiring = true;

        BowClickImage.SetActive(true);
        BowNoClickImage.SetActive(false);

        if (arrowPrefab == null)
        {
            Debug.LogWarning("AI Arrow Prefab is not assigned!");
            yield break;
        }

        // Simulate hold/pull before release (mirror multiplayer auto-hold)
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayRandomBowPull();
        }
        yield return new WaitForSeconds(aiHoldDuration);

        // Always spawn from the shoot point
        Vector3 adjustedSpawn = shootPoint != null ? shootPoint.position : spawnPosition;

        //Particals.SetActive(true);
        //Particals.GetComponent<ParticleSystem>().Play();

        GameObject newArrow = Instantiate(arrowPrefab, adjustedSpawn, Quaternion.identity);

        BowClickImage.SetActive(false);
        BowNoClickImage.SetActive(false);
        BowNoArrowClickImage.SetActive(true);
        // Play release on shot
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

        BowClickImage.SetActive(false);
        BowNoArrowClickImage.SetActive(false);
        BowNoClickImage.SetActive(true);

        //Particals.GetComponent<ParticleSystem>().Stop();
        //Particals.SetActive(false);

        yield return new WaitForSeconds(spawnGateDelay);
        isFiring = false;

        yield return new WaitForSeconds(arrowLifetime);

        if (newArrow != null)
        {
            Destroy(newArrow);
        }
        // cooldown is applied by hit notification only

    }

    // External call when AI arrow actually hits the opponent target/wall
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
}

public enum AIMode
{
    Easy,
    Hard
}