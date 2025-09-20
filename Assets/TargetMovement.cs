using UnityEngine;

public class TargetMovement : MonoBehaviour
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
    
    private Vector3 startPosition;
    private float timeCounter = 0f;


    
    void Start()
    {
        startPosition = transform.position;
        
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
            if (moveUpDown)
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
   
}
