using UnityEngine;

public class AITargetSpeedController : MonoBehaviour
{
    [Header("AI Target Speed Settings")]
    public bool enableAITargetSpeed = true;
    public float baseSpeed = 2f;
    public float speedIncreaseAmount = 1.5f;
    public float speedIncreaseDuration = 2f;
    public float speedDecreaseAmount = 0.5f;
    public float speedDecreaseDuration = 1f;
    
    [Header("Arrow Tracking Settings")]
    public float trackingRange = 3f;
    public float trackingSpeed = 4f;
    public float speedBoostMultiplier = 2f;
    
    [Header("Dynamic Movement Settings")]
    public float moveToArrowSpeed = 5f;
    public float moveToArrowDuration = 1.5f;
    public float returnToOriginalSpeed = 2f;
    public bool useDynamicBehavior = true;
    public float dynamicSpeedMultiplier = 2f;
    public float dynamicDuration = 3f;
    
    private TargetMovement targetMovement;
    private float originalSpeed;
    private bool isSpeedBoosted = false;
    private bool isTrackingArrow = false;
    private bool isMovingToArrow = false;
    private bool isDynamicMode = false;
    private Vector3 targetArrowPosition;
    private Vector3 originalPosition;
    private float lastMovementDirection = 1f;
    private float moveToArrowTimer = 0f;
    private float dynamicTimer = 0f;
    
    void Start()
    {
        targetMovement = GetComponent<TargetMovement>();
        if (targetMovement != null)
        {
            originalSpeed = targetMovement.moveSpeed;
        }
        originalPosition = transform.position;
    }
    
    void Update()
    {
        if (!enableAITargetSpeed || targetMovement == null) return;
        
        // Handle dynamic mode timer (but keep speed constant)
        if (isDynamicMode)
        {
            dynamicTimer += Time.deltaTime;
            if (dynamicTimer >= dynamicDuration)
            {
                EndDynamicMode();
            }
        }
        
        // Only override movement if in special modes
        if (isMovingToArrow)
        {
            MoveToArrowPosition();
        }
        else if (isTrackingArrow)
        {
            MoveTowardsArrow();
        }
        // Otherwise let TargetMovement handle normal movement
    }
    
    // Method to increase target speed when AI shoots
    public void IncreaseTargetSpeed()
    {
        if (!enableAITargetSpeed || targetMovement == null) return;
        
        // Always use dynamic behavior - just increase speed, keep normal movement
        StartDynamicMode();
    }
    
    // Method to decrease target speed
    public void DecreaseTargetSpeed()
    {
        if (!enableAITargetSpeed || targetMovement == null) return;
        
        targetMovement.moveSpeed = Mathf.Max(originalSpeed - speedDecreaseAmount, 0.5f);
        Debug.Log($"AI Target speed decreased to: {targetMovement.moveSpeed}");
        
        // Reset speed after duration
        Invoke(nameof(ResetTargetSpeed), speedDecreaseDuration);
    }
    
    // Method to reset target speed to original
    public void ResetTargetSpeed()
    {
        if (targetMovement != null)
        {
            targetMovement.moveSpeed = originalSpeed;
            isSpeedBoosted = false;
            Debug.Log($"AI Target speed reset to: {targetMovement.moveSpeed}");
        }
    }
    
    // Method to start tracking arrow position
    public void StartTrackingArrow(Vector3 arrowPosition)
    {
        if (!enableAITargetSpeed) return;
        
        targetArrowPosition = arrowPosition;
        isTrackingArrow = true;
        Debug.Log($"AI Target started tracking arrow at: {arrowPosition}");
    }
    
    // Method to move target to arrow spawn position
    public void MoveToArrowSpawnPosition(Vector3 arrowSpawnPosition)
    {
        if (!enableAITargetSpeed) return;
        
        // Always use dynamic behavior - just increase speed, keep normal movement
        StartDynamicMode();
        Debug.Log($"AI Target entered dynamic mode - increased speed for arrow at: {arrowSpawnPosition}");
    }
    
    // Method to stop tracking arrow
    public void StopTrackingArrow()
    {
        isTrackingArrow = false;
        Debug.Log("AI Target stopped tracking arrow");
    }
    
    // Smart movement towards arrow
    void MoveTowardsArrow()
    {
        // Calculate Y direction to arrow position (only Y-axis movement)
        float yDirection = (targetArrowPosition.y - transform.position.y);
        float yDistance = Mathf.Abs(yDirection);
        
        // Check if target is facing backward (opposite to movement direction)
        bool isFacingBackward = (Mathf.Sign(yDirection) * lastMovementDirection) < 0;
        
        // Calculate movement speed
        float currentSpeed = trackingSpeed;
        if (isFacingBackward && yDistance <= trackingRange)
        {
            // Speed boost when facing backward and close to arrow
            currentSpeed *= speedBoostMultiplier;
        }
        
        // Move only on Y-axis towards arrow
        if (yDistance > 0.1f)
        {
            float yMovement = Mathf.Sign(yDirection) * currentSpeed * Time.deltaTime;
            Vector3 newPosition = new Vector3(transform.position.x, transform.position.y + yMovement, transform.position.z);
            transform.position = newPosition;
        }
        
        // Stop tracking if close enough to arrow on Y-axis
        if (yDistance < 0.5f)
        {
            StopTrackingArrow();
        }
    }
    
    // Method to set movement direction (called by TargetMovement)
    public void SetMovementDirection(float direction)
    {
        lastMovementDirection = direction;
    }
    
    // Method to get current movement direction from TargetMovement
    void UpdateMovementDirection()
    {
        if (targetMovement != null && targetMovement.moveUpDown)
        {
            // Calculate movement direction based on current position
            float currentY = transform.position.y;
            float centerY = originalPosition.y;
            
            if (currentY > centerY)
                lastMovementDirection = 1f; // Moving up
            else if (currentY < centerY)
                lastMovementDirection = -1f; // Moving down
        }
    }
    
    // Method to move target to arrow spawn position
    void MoveToArrowPosition()
    {
        moveToArrowTimer += Time.deltaTime;
        
        // Calculate Y direction to arrow spawn position (only Y-axis movement)
        float yDirection = (targetArrowPosition.y - transform.position.y);
        float yDistance = Mathf.Abs(yDirection);
        
        // Move only on Y-axis towards arrow spawn position
        if (yDistance > 0.1f)
        {
            float yMovement = Mathf.Sign(yDirection) * moveToArrowSpeed * Time.deltaTime;
            Vector3 newPosition = new Vector3(transform.position.x, transform.position.y + yMovement, transform.position.z);
            transform.position = newPosition;
        }
        
        // Stop moving to arrow after duration or when close enough on Y-axis
        if (moveToArrowTimer >= moveToArrowDuration || yDistance <= 0.1f)
        {
            isMovingToArrow = false;
            
            // Re-enable TargetMovement
            if (targetMovement != null)
            {
                targetMovement.moveUpDown = true;
            }
            
            // Start tracking the arrow
            StartTrackingArrow(targetArrowPosition);
            
            Debug.Log("AI Target reached arrow spawn Y position, now tracking arrow");
        }
    }
    
    // Start dynamic mode - increase speed while maintaining normal movement
    void StartDynamicMode()
    {
        if (targetMovement != null && !isDynamicMode)
        {
            isDynamicMode = true;
            dynamicTimer = 0f;
            // Ensure TargetMovement is enabled for normal up/down movement
            targetMovement.moveUpDown = true;
            // Increase speed but keep it constant (don't change it back)
            targetMovement.moveSpeed = originalSpeed * dynamicSpeedMultiplier;
            Debug.Log($"AI Target entered dynamic mode - speed increased to: {targetMovement.moveSpeed}");
        }
    }
    
    // End dynamic mode - keep the increased speed constant
    void EndDynamicMode()
    {
        if (targetMovement != null && isDynamicMode)
        {
            isDynamicMode = false;
            // Ensure TargetMovement stays enabled
            targetMovement.moveUpDown = true;
            // Keep the increased speed constant - don't reset it
            Debug.Log($"AI Target dynamic mode ended - speed stays at: {targetMovement.moveSpeed}");
        }
    }
}
