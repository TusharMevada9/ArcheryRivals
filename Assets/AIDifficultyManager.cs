using UnityEngine;

public class AIDifficultyManager : MonoBehaviour
{
    [Header("AI Difficulty Settings")]
    public DifficultyLevel currentDifficulty = DifficultyLevel.Easy;
    
    [Header("Win Chances")]
    [Range(0f, 1f)]
    public float easyWinChance = 0.4f;  // 40% chance to win
    [Range(0f, 1f)]
    public float hardWinChance = 0.6f;  // 60% chance to win
    
    [Header("AI Behavior")]
    public float aiReactionTime = 1.0f;  // AI નો reaction time
    public float aiAccuracy = 0.8f;       // AI ની accuracy
    public bool aiEnabled = true;         // AI enable છે કે નહીં
    
    [Header("Game State")]
    public bool gameWon = false;
    public bool gameLost = false;
    public int totalShots = 0;
    public int successfulShots = 0;
    
    [Header("Score System")]
    public int playerScore = 0;
    public int aiScore = 0;
    public int targetScore = 10;  // જીતવા માટે કેટલા points જોઈએ
    public bool gameEnded = false;
    
    public enum DifficultyLevel
    {
        Easy,
        Hard
    }
    
    void Start()
    {
        Debug.Log("AI Difficulty Manager Started");
        Debug.Log("Current Difficulty: " + currentDifficulty);
        Debug.Log("Easy Win Chance: " + (easyWinChance * 100) + "%");
        Debug.Log("Hard Win Chance: " + (hardWinChance * 100) + "%");
        Debug.Log("AI Reaction Time: " + aiReactionTime + " seconds");
        Debug.Log("AI Accuracy: " + (aiAccuracy * 100) + "%");
    }

    void Update()
    {
        // Check for difficulty change (for testing)
        if (Input.GetKeyDown(KeyCode.E))
        {
            SetDifficulty(DifficultyLevel.Easy);
        }
        if (Input.GetKeyDown(KeyCode.H))
        {
            SetDifficulty(DifficultyLevel.Hard);
        }
        
        // Check for win/lose (for testing)
        if (Input.GetKeyDown(KeyCode.W))
        {
            CheckWinCondition();
        }
        
        // Toggle AI (for testing)
        if (Input.GetKeyDown(KeyCode.A))
        {
            ToggleAI();
        }
    }
    
    public void SetDifficulty(DifficultyLevel difficulty)
    {
        currentDifficulty = difficulty;
        Debug.Log("AI Difficulty changed to: " + currentDifficulty);
        
        if (currentDifficulty == DifficultyLevel.Easy)
        {
            Debug.Log("Easy Mode: " + (easyWinChance * 100) + "% win chance");
            aiReactionTime = 1.5f;  // Slower reaction
            aiAccuracy = 0.6f;      // Lower accuracy
        }
        else if (currentDifficulty == DifficultyLevel.Hard)
        {
            Debug.Log("Hard Mode: " + (hardWinChance * 100) + "% win chance");
            aiReactionTime = 0.5f;  // Faster reaction
            aiAccuracy = 0.9f;      // Higher accuracy
        }
    }
    
    public bool CheckWinCondition()
    {
        if (!aiEnabled)
        {
            Debug.Log("AI is disabled!");
            return false;
        }
        
        float winChance = GetCurrentWinChance();
        float randomValue = Random.Range(0f, 1f);
        
        bool won = randomValue <= winChance;
        
        if (won)
        {
            gameWon = true;
            successfulShots++;
            Debug.Log("🎉 AI WON! 🎉");
            Debug.Log("Random Value: " + randomValue + " <= Win Chance: " + winChance);
        }
        else
        {
            gameLost = true;
            Debug.Log("😞 AI LOST! 😞");
            Debug.Log("Random Value: " + randomValue + " > Win Chance: " + winChance);
        }
        
        totalShots++;
        Debug.Log("Total Shots: " + totalShots + ", Successful: " + successfulShots);
        
        return won;
    }
    
    public float GetCurrentWinChance()
    {
        if (currentDifficulty == DifficultyLevel.Easy)
        {
            return easyWinChance;
        }
        else
        {
            return hardWinChance;
        }
    }
    
    public void ToggleAI()
    {
        aiEnabled = !aiEnabled;
        Debug.Log("AI " + (aiEnabled ? "Enabled" : "Disabled"));
    }
    
    public void ResetGame()
    {
        gameWon = false;
        gameLost = false;
        totalShots = 0;
        successfulShots = 0;
        playerScore = 0;
        aiScore = 0;
        gameEnded = false;
        Debug.Log("AI Game Reset");
    }
    
    // Player score add કરવા માટે
    public void AddPlayerScore(int points = 1)
    {
        if (gameEnded) return;
        
        playerScore += points;
        Debug.Log("Player Score: " + playerScore);
        
        CheckGameEnd();
    }
    
    // AI score add કરવા માટે
    public void AddAIScore(int points = 1)
    {
        if (gameEnded) return;
        
        aiScore += points;
        Debug.Log("AI Score: " + aiScore);
        
        CheckGameEnd();
    }
    
    // Game end check કરવા માટે
    void CheckGameEnd()
    {
        if (playerScore >= targetScore)
        {
            gameEnded = true;
            gameWon = true;
            Debug.Log("🎉 PLAYER WINS! 🎉 Final Score - Player: " + playerScore + ", AI: " + aiScore);
        }
        else if (aiScore >= targetScore)
        {
            gameEnded = true;
            gameLost = true;
            Debug.Log("😞 AI WINS! 😞 Final Score - Player: " + playerScore + ", AI: " + aiScore);
        }
    }
    
    // Current score display કરવા માટે
    public string GetScoreDisplay()
    {
        return "Player: " + playerScore + " | AI: " + aiScore + " | Target: " + targetScore;
    }
    
    public float GetSuccessRate()
    {
        if (totalShots == 0) return 0f;
        return (float)successfulShots / totalShots;
    }
    
    public void SimulateAIShot()
    {
        if (!aiEnabled) return;
        
        Debug.Log("AI is taking a shot...");
        Invoke("CheckWinCondition", aiReactionTime);
    }
}
