using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using DG.Tweening;

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

    [Header("CountDownPanel")]
    public GameObject CountDownPanel;
    public GameObject Image_CountDown1;
    public GameObject Image_CountDown2;
    public GameObject Image_CountDown3;
    public GameObject Image_CountDownGO;

    [Header("Countdown Animation Settings")]
    public float punchScale = 1.3f;
    public float punchDuration = 0.3f;
    public Ease punchEase = Ease.OutBack;

    [Header("Game Timer Settings")]
    public float gameTimeMinutes = 3f; // 3 minutes game time
    public TextMeshProUGUI timerText; // UI text to display timer
    private float gameTimeRemaining; // Remaining time in seconds
    private bool isGameTimerActive = false;

    [Header("Result UI")]
    public GameObject winnerPanel; // Panel to show when game ends
    public Image winImage; // Image for win result
    public Image loseImage; // Image for lose result  
    public Image drawImage; // Image for draw result
    public TextMeshProUGUI finalScoreText; // Text to show final scores

    [Header("Game State")]
    public bool gameEnded = false; // Flag to prevent multiple game end calls
    public bool isMultiplayerMode = false; // Flag to track multiplayer mode

    private double endTime;

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

        // Initialize game timer
        gameTimeRemaining = gameTimeMinutes * 60f; // Convert minutes to seconds
        Debug.Log($"[UIManager] Timer initialized: {gameTimeRemaining} seconds ({gameTimeMinutes} minutes)");
        UpdateTimerDisplay();
    }

    // Called when both players have joined - starts the multiplayer game
    public void OnBothPlayersJoined()
    {
        Debug.Log("[UIManager] OnBothPlayersJoined called - starting multiplayer countdown");

       // waitingForPlayers = false;
        gameTimeRemaining = gameTimeMinutes * 60f;

        Debug.LogError("Both");

    }

    void Update()
    {
        // Update timer if game is active
        if (isGameTimerActive && gameTimeRemaining > 0)
        {
            if (isMultiplayerMode && endTime != 0)
            {
                // --- Multiplayer: use synchronized Unix endTime ---
                double now = GetUnixTimeNow();
                gameTimeRemaining = (float)(endTime - now);
            }
            else
            {
                // --- Singleplayer: local countdown ---
                gameTimeRemaining -= Time.deltaTime;
            }

            UpdateTimerDisplay();

            // Check if time is up
            if (gameTimeRemaining <= 0)
            {
                gameTimeRemaining = 0;
                OnGameTimeUp();
            }
        }
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

    // Method to start countdown and set isGameStart = true
    public void CountDownStart()
    {
        Debug.Log("[UIManager] Starting countdown...");
        StartCoroutine(CountDownSequence());
    }

    // Punch animation method for countdown images
    private void PlayPunchAnimation(GameObject imageObject)
    {
        if (imageObject != null)
        {
            // Reset scale to normal
            imageObject.transform.localScale = Vector3.one;

            // Punch animation - scale up and bounce back
            imageObject.transform.DOPunchScale(Vector3.one * (punchScale - 1f), punchDuration, 10, 1f)
                .SetEase(punchEase);
        }
    }

    // Timer display and control methods
    private void UpdateTimerDisplay()
    {
        if (timerText != null)
        {
            int minutes = Mathf.FloorToInt(gameTimeRemaining / 60f);
            int seconds = Mathf.FloorToInt(gameTimeRemaining % 60f);

            // Format as MM:SS
            string timeString = string.Format("{0:00}:{1:00}", minutes, seconds);
            timerText.text = timeString;

            // Change color to red when 30 seconds or less remain
            if (gameTimeRemaining <= 30f)
            {
                timerText.color = Color.red;
            }
            else
            {
                timerText.color = Color.black;
            }
        }
        else
        {
            Debug.LogWarning("[UIManager] Timer text component is not assigned!");
        }
    }

    // Start the game timer
    public void StartGameTimer()
    {
        isGameTimerActive = true;
        Debug.Log("[UIManager] Game timer started - 3 minutes countdown begins!");
        Debug.Log($"[UIManager] Timer text component: {(timerText != null ? "Assigned" : "NULL")}");
    }

    // Stop the game timer
    public void StopGameTimer()
    {
        isGameTimerActive = false;
        Debug.Log("[UIManager] Game timer stopped!");
    }

    // Called when game time is up
    private void OnGameTimeUp()
    {
        Debug.Log("[UIManager] Game time is up! Game over!");
        isGameTimerActive = false;

        // End the game and determine winner
        if (isMultiplayerMode)
        {
            EndGameMultiplayer();
        }
        else
        {
            EndGame();
        }
    }

    // Get remaining time (for other scripts to access)
    public float GetRemainingTime()
    {
        return gameTimeRemaining;
    }

    // Check if timer is active
    public bool IsTimerActive()
    {
        return isGameTimerActive;
    }
    double GetUnixTimeNow()
    {
        return (System.DateTime.UtcNow - new System.DateTime(1970, 1, 1)).TotalSeconds;
    }
    IEnumerator CountDownSequence()
    {
        // Show countdown panel
        CountDownPanel.SetActive(true);

        // Hide all countdown images initially
        if (Image_CountDown1 != null) Image_CountDown1.SetActive(false);
        if (Image_CountDown2 != null) Image_CountDown2.SetActive(false);
        if (Image_CountDown3 != null) Image_CountDown3.SetActive(false);
        if (Image_CountDownGO != null) Image_CountDownGO.SetActive(false);

        // Show "3" with punch animation
        Debug.Log("[UIManager] Countdown: 3");
        if (Image_CountDown3 != null)
        {
            Image_CountDown3.SetActive(true);
            PlayPunchAnimation(Image_CountDown3);
        }
        yield return new WaitForSeconds(1f);

        // Hide "3" and show "2" with punch animation
        Debug.Log("[UIManager] Countdown: 2");
        if (Image_CountDown3 != null) Image_CountDown3.SetActive(false);
        if (Image_CountDown2 != null)
        {
            Image_CountDown2.SetActive(true);
            PlayPunchAnimation(Image_CountDown2);
        }
        yield return new WaitForSeconds(1f);

        // Hide "2" and show "1" with punch animation
        Debug.Log("[UIManager] Countdown: 1");
        if (Image_CountDown2 != null) Image_CountDown2.SetActive(false);
        if (Image_CountDown1 != null)
        {
            Image_CountDown1.SetActive(true);
            PlayPunchAnimation(Image_CountDown1);
        }
        yield return new WaitForSeconds(1f);

        // Hide "1" and show "GO!" with bigger punch animation
        Debug.Log("[UIManager] Countdown: GO!");
        if (Image_CountDown1 != null) Image_CountDown1.SetActive(false);
        if (Image_CountDownGO != null)
        {
            Image_CountDownGO.SetActive(true);
            // Bigger punch for GO!
            Image_CountDownGO.transform.localScale = Vector3.one;
            Image_CountDownGO.transform.DOPunchScale(Vector3.one * (punchScale * 1.5f - 1f), punchDuration * 1.5f, 15, 1f)
                .SetEase(Ease.OutBounce);
        }
        yield return new WaitForSeconds(1f);

        // Hide countdown panel
        CountDownPanel.SetActive(false);

        if (isMultiplayerMode)
        {
            endTime = GetUnixTimeNow() + gameTimeRemaining;
            Debug.Log($"[UIManager] Multiplayer timer started - endTime set to {endTime}, gameTimeRemaining: {gameTimeRemaining}");
        }

        // Set isGameStart = true for AI shooting
        isGameStart = true;
        Debug.Log("[UIManager] Countdown finished! isGameStart = true - AI can now start shooting!");

        // Start the 3-minute game timer
        StartGameTimer();
    }

    // Test method to manually start timer (for debugging)
    [ContextMenu("Test Start Timer")]
    public void TestStartTimer()
    {
        Debug.Log("[UIManager] Testing timer start...");
        StartGameTimer();
    }

    // End the game and determine winner
    private void EndGame()
    {
        if (gameEnded) return; // Prevent multiple calls
        gameEnded = true;

        // Stop the game
        isGameStart = false;
        isGameTimerActive = false;

        Debug.Log($"[UIManager] Game ended! Final scores - Red: {redScore}, Blue: {blueScore}");

        // Determine winner and show result
        if (redScore > blueScore)
        {
            // Red wins
            ShowResult("won", redScore, blueScore);
            Debug.Log("[UIManager] 🎉 RED WINS! 🎉");
        }
        else if (blueScore > redScore)
        {
            // Blue wins  
            ShowResult("lost", redScore, blueScore);
            Debug.Log("[UIManager] 😞 BLUE WINS! 😞");
        }
        else
        {
            // Draw
            ShowResult("draw", redScore, blueScore);
            Debug.Log("[UIManager] 🤝 IT'S A DRAW! 🤝");
        }
    }

    // End the game and determine winner (Multiplayer)
    private void EndGameMultiplayer()
    {
        if (gameEnded) return; // Prevent multiple calls
        gameEnded = true;

        // Stop the game
        isGameStart = false;
        isGameTimerActive = false;

        Debug.Log($"[UIManager] Multiplayer game ended! Final scores - Red: {redScore}, Blue: {blueScore}");

        // Get local player ID
        int localPid = -1;
        if (FusionConnector.instance != null && FusionConnector.instance.NetworkRunner != null)
        {
            localPid = FusionConnector.instance.NetworkRunner.LocalPlayer.PlayerId;
        }

        Debug.Log($"[UIManager] Local Player ID: {localPid}");

        // Determine result based on local player perspective
        if (redScore > blueScore)
        {
            // Red wins
            if (localPid == 1) // Local player is Red
            {
                ShowResult("won", redScore, blueScore);
                Debug.Log("[UIManager] 🎉 YOU WIN! (Red > Blue) 🎉");
            }
            else if (localPid == 2) // Local player is Blue
            {
                ShowResult("lost", redScore, blueScore);
                Debug.Log("[UIManager] 😞 YOU LOSE! (Red > Blue) 😞");
            }
            else
            {
                ShowResult("won", redScore, blueScore); // Fallback
                Debug.Log("[UIManager] 🎉 RED WINS! 🎉");
            }
        }
        else if (blueScore > redScore)
        {
            // Blue wins
            if (localPid == 1) // Local player is Red
            {
                ShowResult("lost", redScore, blueScore);
                Debug.Log("[UIManager] 😞 YOU LOSE! (Blue > Red) 😞");
            }
            else if (localPid == 2) // Local player is Blue
            {
                ShowResult("won", redScore, blueScore);
                Debug.Log("[UIManager] 🎉 YOU WIN! (Blue > Red) 🎉");
            }
            else
            {
                ShowResult("lost", redScore, blueScore); // Fallback
                Debug.Log("[UIManager] 😞 BLUE WINS! 😞");
            }
        }
        else
        {
            // Draw
            ShowResult("draw", redScore, blueScore);
            Debug.Log("[UIManager] 🤝 IT'S A DRAW! 🤝");
        }
    }

    // Show result UI and send to server
    private void ShowResult(string outcome, int playerScore, int opponentScore)
    {
        // Show winner panel
        if (winnerPanel != null)
        {
            winnerPanel.SetActive(true);
        }

        // Hide all result images first
        if (winImage != null) winImage.gameObject.SetActive(false);
        if (loseImage != null) loseImage.gameObject.SetActive(false);
        if (drawImage != null) drawImage.gameObject.SetActive(false);

        // Show appropriate result image
        if (outcome == "won" && winImage != null)
        {
            winImage.gameObject.SetActive(true);
        }
        else if (outcome == "lost" && loseImage != null)
        {
            loseImage.gameObject.SetActive(true);
        }
        else if (outcome == "draw" && drawImage != null)
        {
            drawImage.gameObject.SetActive(true);
        }

        // Show final score
        if (finalScoreText != null)
        {
            finalScoreText.text = $"Final Score:\nRed: {playerScore}\nBlue: {opponentScore}";
        }

        // Send result to server with delay
        StartCoroutine(SendMatchResultToServerDelayed(outcome, playerScore, opponentScore, 3f));
    }

    // Send match result to server after delay
    private IEnumerator SendMatchResultToServerDelayed(string outcome, int playerScore, int opponentScore, float delaySeconds)
    {
        Debug.Log($"[UIManager] 🚀 Scheduling server result: {outcome} (Red: {playerScore}, Blue: {opponentScore}) in {delaySeconds} seconds");

        yield return new WaitForSeconds(delaySeconds);

        // Send to server via IFrameBridge
        if (IFrameBridge.Instance != null)
        {
            IFrameBridge.Instance.SendMatchResultToPlatform(outcome, playerScore, opponentScore);
            Debug.Log($"[UIManager] ✅ Match result sent to server: {outcome}");
        }
        else
        {
            Debug.LogWarning("[UIManager] ❌ IFrameBridge not found - cannot send result to server");
        }
    }

    // Public method to manually end game (for testing)
    [ContextMenu("Test End Game")]
    public void TestEndGame()
    {
        Debug.Log("[UIManager] Testing game end...");
        EndGame();
    }

    // Reset game for new match
    public void ResetGame()
    {
        gameEnded = false;
        isGameStart = false;
        isGameTimerActive = false;
        redScore = 0;
        blueScore = 0;

        // Reset UI
        UpdateRedScore(0);
        UpdateBlueScore(0);

        // Hide result panel
        if (winnerPanel != null)
        {
            winnerPanel.SetActive(false);
        }

        // Hide all result images
        if (winImage != null) winImage.gameObject.SetActive(false);
        if (loseImage != null) loseImage.gameObject.SetActive(false);
        if (drawImage != null) drawImage.gameObject.SetActive(false);

        // Reset timer
        gameTimeRemaining = gameTimeMinutes * 60f;
        UpdateTimerDisplay();

        Debug.Log("[UIManager] Game reset for new match!");
    }
}
