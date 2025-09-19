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

    public bool isGameStart = false;

    public static UIManager Instance { get; private set; }

    void Awake()
    {
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

    void Start()
    {
        UpdateRedScore(0);
        UpdateBlueScore(0);
    }

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
