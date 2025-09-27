using UnityEngine;

public class AITargetMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 2f;   
    public float moveRange = 3f;
    public bool moveUpDown = true;
    
    [Header("Random Speed Settings")]
    public bool useRandomSpeed = true;
    public float minSpeed = 1f;
    public float maxSpeed = 4f;
    public float speedChangeInterval = 1f;
    
    [Header("Dynamic Movement Settings")]
    public bool useDynamicMovement = true;
    public float moveToArrowSpeed = 5f;
    public float moveToArrowDuration = 1.5f;
    public float dynamicSpeedMultiplier = 2f;
    public float dynamicDuration = 3f;
    
    private Vector3 startPosition;
    private float timeCounter = 0f;
    private bool isMovingToArrow = false;
    private bool isDynamicMode = false;
    private Vector3 targetArrowPosition;
    private float moveToArrowTimer = 0f;
    private float dynamicTimer = 0f;
    private float originalSpeed;
    
    void Start()
    {
        startPosition = transform.position;
        originalSpeed = moveSpeed;
        
        if (useRandomSpeed)
        {
            moveSpeed = Random.Range(minSpeed, maxSpeed);
            InvokeRepeating(nameof(ChangeRandomSpeed), speedChangeInterval, speedChangeInterval);
        }
    }
    
    void Update()
    {
        if (UIManager.Instance.isGameStart == true)
        {
            // Handle dynamic mode timer
            if (isDynamicMode)
            {
                dynamicTimer += Time.deltaTime;
                if (dynamicTimer >= dynamicDuration)
                {
                    EndDynamicMode();
                }
            }
            
            if (isMovingToArrow)
            {
                MoveToArrowPosition();
            }
            else if (moveUpDown)
            {
                timeCounter += Time.deltaTime * moveSpeed;

                float newY = startPosition.y + Mathf.Sin(timeCounter) * moveRange;

                transform.position = new Vector3(startPosition.x, newY, startPosition.z);
            }
        }
    }

    void ChangeRandomSpeed()
    {
        if (useRandomSpeed)
        {
            moveSpeed = Random.Range(minSpeed, maxSpeed);
        }
    }
    
    // Method to move target to arrow spawn position when AI shoots
    public void MoveToArrowSpawnPosition(Vector3 arrowSpawnPosition)
    {
        if (!useDynamicMovement) return;
        
        targetArrowPosition = arrowSpawnPosition;
        isMovingToArrow = true;
        moveToArrowTimer = 0f;
        
        Debug.Log($"AI Target moving to arrow spawn position: {arrowSpawnPosition}");
    }
    
    // Method to increase target speed when AI shoots
    public void IncreaseTargetSpeed()
    {
        if (!useDynamicMovement) return;
        
        StartDynamicMode();
        Debug.Log($"AI Target speed increased");
    }
    
    // Move target to arrow spawn position
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
            
            // Start dynamic mode for increased speed
            StartDynamicMode();
            
            Debug.Log("AI Target reached arrow spawn Y position, now in dynamic mode");
        }
    }
    
    // Start dynamic mode - increase speed while maintaining normal movement
    void StartDynamicMode()
    {
        if (!isDynamicMode)
        {
            isDynamicMode = true;
            dynamicTimer = 0f;
            moveSpeed = originalSpeed * dynamicSpeedMultiplier;
            Debug.Log($"AI Target entered dynamic mode - speed increased to: {moveSpeed}");
        }
    }
    
    // End dynamic mode - return to normal speed
    void EndDynamicMode()
    {
        if (isDynamicMode)
        {
            isDynamicMode = false;
            moveSpeed = originalSpeed;
            Debug.Log($"AI Target exited dynamic mode - speed returned to: {moveSpeed}");
        }
    }
}
