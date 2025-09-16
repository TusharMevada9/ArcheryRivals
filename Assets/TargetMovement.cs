using UnityEngine;

public class TargetMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 2f;           // ગતિની ઝડપ
    public float moveRange = 3f;           // કેટલી દૂર ઉપર-નીચે જશે
    public bool moveUpDown = true;         // ઉપર-નીચે move કરવું છે કે નહીં
    
    [Header("Random Speed Settings")]
    public bool useRandomSpeed = true;     // random speed ચાલુ કરવું છે કે નહીં
    public float minSpeed = 1f;            // minimum speed
    public float maxSpeed = 4f;            // maximum speed
    public float speedChangeInterval = 1f; // કેટલા સેકન્ડ પછી speed બદલાશે (1 second)
    
    private Vector3 startPosition;         // શરૂઆતની position
    private float timeCounter = 0f;        // સમયનો કાઉન્ટર


    
    void Start()
    {
        // શરૂઆતમાં જે position છે તે store કરો
        startPosition = transform.position;
        
        // શરૂઆતમાં random speed set કરો
        if (useRandomSpeed)
        {
            moveSpeed = Random.Range(minSpeed, maxSpeed);
            // InvokeRepeating નો ઉપયોગ કરીને દર 1 સેકન્ડે speed change કરો
            InvokeRepeating(nameof(ChangeRandomSpeed), speedChangeInterval, speedChangeInterval);
        }
    }
    
    void Update()
    {
        if (moveUpDown)
        {
            // સમયનો કાઉન્ટર વધારો
            timeCounter += Time.deltaTime * moveSpeed;
            
            // Y position ને ઉપર-નીચે move કરો
            float newY = startPosition.y + Mathf.Sin(timeCounter) * moveRange;
            
            // નવી position set કરો
            transform.position = new Vector3(startPosition.x, newY, startPosition.z);
        }
    }
    
    // InvokeRepeating માટેનો ફંક્શન - દર 1 સેકન્ડે speed change કરે છે
    void ChangeRandomSpeed()
    {
        if (useRandomSpeed)
        {
            moveSpeed = Random.Range(minSpeed, maxSpeed);
        }
    }
   
}
