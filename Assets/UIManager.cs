using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    [Header("Score Text Elements")]
    public TextMeshProUGUI redScoreText;
    public TextMeshProUGUI blueScoreText;
    
    [Header("Score Variables")]
    public int redScore = 0;
    public int blueScore = 0;

    // Singleton Instance
    public static UIManager Instance { get; private set; }

    void Awake()
    {
        // Singleton setup
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        UpdateRedScore(0);
        UpdateBlueScore(0);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    // Red Score Functions
    public void UpdateRedScore(int score)
    {
        redScore = score;
        if (redScoreText != null)
            redScoreText.text = "Red Score: " + redScore.ToString();
    }
    
    public void AddRedScore(int points)
    {
        UpdateRedScore(redScore + points);
    }
    
    // Blue Score Functions
    public void UpdateBlueScore(int score)
    {
        blueScore = score;
        if (blueScoreText != null)
            blueScoreText.text = "Blue Score: " + blueScore.ToString();
    }
    
    public void AddBlueScore(int points)
    {
        UpdateBlueScore(blueScore + points);
    }
}
