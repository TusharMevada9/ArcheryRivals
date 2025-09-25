using System;
using System.Runtime.InteropServices;
using UnityEngine;
using static Unity.Collections.Unicode;

public class IFrameBridge : MonoBehaviour
{
    public static IFrameBridge Instance { get; private set; }

    // Score submission tracking
    private bool scoreSubmitted = false;
    private float submitTimeout = 5f;
    private float submitTimer = 0f;

    // Match information
    public static string MatchId { get; private set; } = string.Empty;
    public static string PlayerId { get; private set; } = string.Empty;
    public static string OpponentId { get; private set; } = string.Empty;
    public static string Region { get; private set; } = string.Empty;

    // Bot difficulty from platform
    internal AIMode botLevel;
    public GameType gameType;

    private bool isInitialized = false;
    private bool gameModeInitialized = false;

    // WebGL external methods - JavaScript bridge functions
#if UNITY_WEBGL && !UNITY_EDITOR
	[DllImport("__Internal")]
	private static extern string GetURLParameters();

	[DllImport("__Internal")]
	private static extern void SendMatchResult(
		string matchId,
		string playerId,
		string opponentId,
		string outcome,
		int score,
		int opponentScore,
		int averagePing,
		string region
	);

	[DllImport("__Internal")]
	private static extern void SendMatchAbort(string message, string error, string errorCode);

	[DllImport("__Internal")]
	private static extern void SendGameState(string state);

	[DllImport("__Internal")]
	private static extern void SendBuildVersion(string version);

	[DllImport("__Internal")]
	private static extern void SendGameReady();

	[DllImport("__Internal")]
	private static extern int IsMobileWeb();
#else
	// Fallback methods for non-WebGL builds
	private static string GetURLParameters() { return "{}"; }
	private static void SendMatchResult(string matchId, string playerId, string opponentId, string outcome, int score, int opponentScore, int averagePing, string region) { }
	private static void SendMatchAbort(string message, string error, string errorCode) { }
	private static void SendGameState(string state) { }
	private static void SendBuildVersion(string version) { }
	private static void SendGameReady() { }
	private static int IsMobileWeb() { return 0; }
#endif

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            gameObject.name = "IFrameBridge";
            DontDestroyOnLoad(gameObject);
            Debug.unityLogger.logEnabled = true;
            isInitialized = true;
            gameModeInitialized = false; // Reset game mode initialization
            Debug.Log("[IFrameBridge] Instance initialized");
        }
        else
        {
            Debug.LogWarning("[IFrameBridge] Multiple instances detected. Destroying duplicate.");
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        if (!isInitialized)
        {
            Debug.LogError("[IFrameBridge] Start called before initialization!");
            return;
        }

        try
        {
#if UNITY_WEBGL && !UNITY_EDITOR
			// In WebGL build, follow platform flow:
			// 1. Send ready signal
			// 2. Get match parameters from URL 
			// 3. Initialize appropriate mode
			Debug.Log("[IFrameBridge] Sending game ready signal...");
			SendGameReady();
			SendBuildVersion(Application.version);
			Debug.Log("[IFrameBridge] Game ready signal sent successfully");

			ExtractParametersFromURL();
#else
            // In Unity Editor or non-WebGL builds, start with test parameters
            Debug.Log(
                "[IFrameBridge] Editor/Non-WebGL mode - starting with test parameters"
            );
            ExtractParametersFromURL();

            // Debug: Check if we're in AI mode
            Debug.Log($"[IFrameBridge] Game type after initialization: {gameType}");
            Debug.Log($"[IFrameBridge] Bot level: {botLevel}");
#endif
        }
        catch (Exception e)
        {
            Debug.LogError("[IFrameBridge] Error in Start: " + e.Message);
        }
    }

    private void ExtractParametersFromURL()
    {
        //try
        //{
#if UNITY_WEBGL && !UNITY_EDITOR
        // Get parameters from URL in WebGL build
        string json = GetURLParameters();
        if (string.IsNullOrEmpty(json))
        {
            Debug.LogWarning("[IFrameBridge] No URL parameters found in WebGL build, using fallback AI mode");
            // Fallback to AI mode if no URL parameters (no local region default)
            string fallbackJson = "{\"matchId\":\"webgl_fallback\",\"playerId\":\"webgl_player\",\"opponentId\":\"b912345678\"}";
            InitParamsFromJS(fallbackJson);
            return;
        }
#else
        // Use test data in editor/non-WebGL builds - CHOOSE MODE HERE:
        // FOR AI MODE TESTING (uncomment this line):
        //string json = "{\"matchId\":\"test_match\",\"playerId\":\"human_player\",\"opponentId\":\"b912345678\"}";

        //FOR MULTIPLAYER MODE TESTING (comment out the line above and uncomment this line):
        string json = "{\"matchId\":\"test_match\",\"playerId\":\"player1\",\"opponentId\":\"player2\"}";

        Debug.Log("Enter");

        InitParamsFromJS(json);
#endif
        //}
        //catch (Exception e)
        //{
        //	Debug.LogError("[IFrameBridge] Error extracting URL parameters: " + e.Message);
        //	// Fallback to AI mode on error
        //	Debug.Log("[IFrameBridge] Using fallback AI mode due to error");
        //	string fallbackJson = "{\"matchId\":\"error_fallback\",\"playerId\":\"fallback_player\",\"opponentId\":\"b912345678\"}";
        //	InitParamsFromJS(fallbackJson);
        //}
    }

    public bool IsMobileWebGL()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
		try { return IsMobileWeb() == 1; } catch { return false; }
#else
        return Application.isMobilePlatform;
#endif
    }

    private void StartTestMatch()
    {
        Debug.Log("[IFrameBridge] Starting test match...");

        // CHOOSE ONE MODE FOR TESTING:

        // 1. FOR AI MODE TESTING (default):
        //string json = "{\"matchId\":\"test_match\",\"playerId\":\"human_player\",\"opponentId\":\"a9\"}";


        // 2. FOR MULTIPLAYER MODE TESTING (uncomment this line and comment out the line above):
        string json = "{\"matchId\":\"test_match\",\"playerId\":\"player1\",\"opponentId\":\"player2\",\"region\":\"in\"}";

        InitParamsFromJS(json);
    }

    public void InitParamsFromJS(string json)
    {

        Debug.Log("Enter");

        if (string.IsNullOrEmpty(json))
        {
            Debug.LogError("[IFrameBridge] Received null or empty JSON!");
            AbortInitializationError("Empty JSON received");
            return;
        }

        try
        {
            Debug.Log($"[IFrameBridge] Parsing match parameters from JSON: {json}");

            var data = JsonUtility.FromJson<MatchParams>(json);
            if (data == null)
            {
                throw new ArgumentException("Failed to parse JSON data");
            }

            if (
                string.IsNullOrEmpty(data.matchId)
                || string.IsNullOrEmpty(data.playerId)
                || string.IsNullOrEmpty(data.opponentId)
            )
            {
                throw new ArgumentException("Required match parameters are missing or empty");
            }

            MatchId = data.matchId;
            PlayerId = data.playerId;
            OpponentId = data.opponentId;
            
            // Handle region - default to "in" if not provided
            Region = string.IsNullOrEmpty(data.region) ? "in" : data.region.Trim().ToLower();
            
            Debug.Log($"[IFrameBridge] Region received: '{Region}'");

            Debug.Log(
                $"[IFrameBridge] Match parameters set - Match ID: {MatchId}, Player ID: {PlayerId}, Opponent ID: {OpponentId}, Region: {Region}"
            );

            bool isOpponentBot = IsBot(OpponentId);

            Debug.Log(
                $"[IFrameBridge] ===== MODE DETECTION ===== OpponentId: '{OpponentId}', IsBot: {isOpponentBot}"
            );

            if (isOpponentBot)
            {
                botLevel = GetBotLevel(OpponentId);
                gameType = GameType.Singleplayer;
                gameModeInitialized = true;

                Debug.Log(
                    $"[IFrameBridge] AI MODE INITIALIZED - Bot difficulty: {botLevel}, gameType: {gameType}"
                );

                InitializeAIMode();
            }
            else
            {
                gameType = GameType.Multiplayer;
                gameModeInitialized = true;
                FusionConnector.instance.ConnectToServer(MatchId, Region);
                Debug.Log($"[IFrameBridge] MULTIPLAYER MODE INITIALIZED - gameType: {gameType}, Region: {Region}");

            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[IFrameBridge] Error parsing match parameters: {e.Message}");
            AbortInitializationError($"Match parameter parsing failed: {e.Message}");
        }
    }

    private void InitializeAIMode()
    {
        Debug.Log("[IFrameBridge] Initializing AI Mode...");

        if (GameInitializer.Instance != null)
        {
            GameInitializer.Instance.InitializeMatch();
        }
        else
        {
            Debug.LogError("[IFrameBridge] GameInitializer not found for AI mode!");
            AbortGameStartFailure("GameInitializer not found for AI mode");
        }
    }

    private void InitializeMultiplayerMode()
    {
        Debug.Log("[IFrameBridge] Initializing Multiplayer Mode...");

        if (FusionConnector.instance != null)
        {
            FusionConnector.instance.ConnectToServer(MatchId, Region);
        }
        else
        {
            Debug.LogError("[IFrameBridge] FusionConnector not found for multiplayer mode!");
            AbortGameStartFailure("FusionConnector not found for multiplayer mode");
        }
    }

    public bool IsBot(string playerId)
    {
        if (string.IsNullOrEmpty(playerId))
            return false;
        // According to documentation, bot IDs start with "a9" or "b9"
        return playerId.StartsWith("a9") || playerId.StartsWith("b9");
    }

    public AIMode GetBotLevel(string playerId)
    {
        if (playerId.StartsWith("a9"))
            return AIMode.Easy;
        else if (playerId.StartsWith("b9"))
            return AIMode.Hard;
        return AIMode.Easy;
    }

    public void PostMatchResult(string outcome, int score, int opponentScore)
    {
        Debug.Log(
            "[IFrameBridge] Match Result - Outcome: "
                + outcome
                + ", Score: "
                + score.ToString()
                + ", OpponentScore: "
                + opponentScore.ToString()
        );

        // Debug: Check if match parameters are set
        Debug.Log($"[IFrameBridge] Match Parameters - MatchId: '{MatchId}', PlayerId: '{PlayerId}', OpponentId: '{OpponentId}'");
        
        if (string.IsNullOrEmpty(MatchId) || string.IsNullOrEmpty(PlayerId) || string.IsNullOrEmpty(OpponentId))
        {
            Debug.LogError("[IFrameBridge] Match parameters are not set! Cannot send match result to server.");
            return;
        }

        scoreSubmitted = false;
        submitTimer = 0f;

#if UNITY_WEBGL && !UNITY_EDITOR
		Debug.Log($"[IFrameBridge] Sending match result to WebGL server: {outcome}, Region: {Region}");
		SendMatchResult(MatchId, PlayerId, OpponentId, outcome, score, opponentScore, 0, Region);
		StartCoroutine(WaitForScoreSubmission());
#else
        Debug.Log($"[IFrameBridge] Non-WebGL build - simulating match result submission");
        // In editor or non-WebGL builds, simulate submission after a short delay
        StartCoroutine(SimulateScoreSubmission());
#endif
    }

    // 🚀 NEW: Send match result after 3-second delay
    public void PostMatchResultDelayed(string outcome, int score, int opponentScore, float delaySeconds = 3f)
    {
        Debug.Log($"[IFrameBridge] 🚀 Scheduling delayed match result: {outcome} (Score: {score}, Opponent: {opponentScore}) in {delaySeconds} seconds");
        StartCoroutine(PostMatchResultWithDelay(outcome, score, opponentScore, delaySeconds));
    }

    // 🚀 Coroutine to send match result after delay
    private System.Collections.IEnumerator PostMatchResultWithDelay(string outcome, int score, int opponentScore, float delaySeconds)
    {
        Debug.Log($"[IFrameBridge] 🚀 Waiting {delaySeconds} seconds before sending match result...");
        yield return new WaitForSeconds(delaySeconds);

        Debug.Log($"[IFrameBridge] 🚀 Delay complete! Now sending match result: {outcome}");
        PostMatchResult(outcome, score, opponentScore);
    }

    public void PostGameState(string state)
    {
        Debug.Log("[IFrameBridge] Game State: " + state);

#if UNITY_WEBGL && !UNITY_EDITOR
		SendGameState(state);
#else
        Debug.Log($"[IFrameBridge] Game State (non-WebGL): {state}");
#endif
    }

    private System.Collections.IEnumerator WaitForScoreSubmission()
    {
        while (submitTimer < submitTimeout && !scoreSubmitted)
        {
            submitTimer += Time.deltaTime;
            yield return null;
        }

        if (!scoreSubmitted)
        {
            Debug.LogWarning("[IFrameBridge] Score submission timed out");
        }
    }

    private System.Collections.IEnumerator SimulateScoreSubmission()
    {
        yield return new WaitForSeconds(0.5f);
        scoreSubmitted = true;
        Debug.Log("[IFrameBridge] Score submission simulated in editor");
    }

    // Called from JavaScript when score is submitted successfully
    public void OnScoreSubmitted()
    {
        scoreSubmitted = true;
        Debug.Log("[IFrameBridge] Score submitted successfully");
    }

    public bool IsScoreSubmitted()
    {
        return scoreSubmitted;
    }

    // Public method to reset game mode for testing
    public void ResetGameMode()
    {
        gameModeInitialized = false;
        Debug.Log("[IFrameBridge] Game mode reset for testing");
    }

    public void PostMatchAbort(string message, string error = "", string errorCode = "")
    {
        Debug.Log(
            "[IFrameBridge] Match Aborted - Message: "
                + message
                + ", Error: "
                + error
                + ", Code: "
                + errorCode
        );

#if UNITY_WEBGL && !UNITY_EDITOR
		SendMatchAbort(message, error, errorCode);
#endif
    }

    // Convenience methods for common abort scenarios
    public void AbortGameStartFailure(string reason)
    {
        PostMatchAbort($"Game failed to start: {reason}", reason, "GAME_START_FAILURE");
    }

    public void AbortPlayerDisconnect(string playerId)
    {
        PostMatchAbort($"Player {playerId} disconnected", "Player disconnected", "PLAYER_DISCONNECT");
    }

    // Handle opponent forfeit scenario - using Maze-Muncher's simple approach
    public void PostOpponentForfeit(string opponentId)
    {
        Debug.Log($"[IFrameBridge] Opponent {opponentId} forfeited the match");

        // Send match result FIRST - this shows the win popup
        UIManager uiManager = UnityEngine.Object.FindFirstObjectByType<UIManager>();
        if (uiManager != null)
        {
            Debug.Log("[IFrameBridge] Sending WIN result for opponent forfeit");
            uiManager.StartCoroutine(uiManager.SendMatchResultToPlatformDelayed("won", 
                uiManager.GetLocalPlayerScoreForReport(), 0.1f, uiManager.GetOpponentScoreForReport()));
        }
        else
        {
            Debug.LogWarning("[IFrameBridge] UIManager not found - cannot send win result");
        }

        // Also send abort message for cleanup
        PostMatchAbort("Opponent left the game.", "", "");
    }

    // Handle when local player leaves (should trigger opponent win)
    public void PostPlayerForfeit()
    {
        Debug.Log("[IFrameBridge] Local player forfeited the match");

        // Send match result FIRST - this shows the lose popup
        UIManager uiManager = UnityEngine.Object.FindFirstObjectByType<UIManager>();
        if (uiManager != null)
        {
            Debug.Log("[IFrameBridge] Sending LOSE result for player forfeit");
            uiManager.StartCoroutine(uiManager.SendMatchResultToPlatformDelayed("lost", 
                uiManager.GetLocalPlayerScoreForReport(), 0.1f, uiManager.GetOpponentScoreForReport()));
        }
        else
        {
            Debug.LogWarning("[IFrameBridge] UIManager not found - cannot send lose result");
        }

        // Also send abort message for cleanup
        PostMatchAbort("You left the game.", "", "");
    }

    public void AbortCriticalError(string error, string errorCode = "CRITICAL_ERROR")
    {
        PostMatchAbort($"Critical error occurred: {error}", error, errorCode);
    }

    public void AbortConnectionError(string error)
    {
        PostMatchAbort("Connection error", error, "CONNECTION_ERROR");
    }

    public void AbortInitializationError(string error)
    {
        PostMatchAbort("Failed to initialize game", error, "INIT_ERROR");
    }

    // JavaScript callback methods
    public void OnGamePaused()
    {
        Debug.Log("[IFrameBridge] Game paused by platform");
    }

    public void OnGameResumed()
    {
        Debug.Log("[IFrameBridge] Game resumed by platform");
    }

    public void OnConnectionLost()
    {
        AbortConnectionError("Network connection lost");
        Debug.Log("[IFrameBridge] Connection lost");
    }

    // Additional abort scenarios
    public void OnPlayerTimeout(string playerId)
    {
        AbortPlayerDisconnect($"Player {playerId} timed out");
        Debug.Log($"[IFrameBridge] Player {playerId} timed out");
    }

    public void OnGameCrash(string error)
    {
        AbortCriticalError($"Game crashed: {error}", "GAME_CRASH");
        Debug.LogError($"[IFrameBridge] Game crash detected: {error}");
    }

    public void OnResourceLoadFailure(string resource)
    {
        AbortGameStartFailure($"Failed to load required resource: {resource}");
        Debug.LogError($"[IFrameBridge] Resource load failure: {resource}");
    }

    public void OnInvalidGameState(string state)
    {
        AbortCriticalError($"Invalid game state detected: {state}", "INVALID_STATE");
        Debug.LogError($"[IFrameBridge] Invalid game state: {state}");
    }

    // Method to handle when player wants to leave the game
    public void OnPlayerLeaveGame()
    {
        Debug.Log("[IFrameBridge] Player requested to leave the game");

        // Send forfeit message to platform
        PostPlayerForfeit();

        // Disconnect from network if in multiplayer
        if (gameType == GameType.Multiplayer && FusionConnector.instance != null)
        {
            FusionConnector.instance.NetworkRunner?.Shutdown();
        }

        // Stop the game (no direct GameManager flag here)
        Debug.Log("[IFrameBridge] Player left - stopping session requested");
    }

    // Back-compat helper for UIManager to send minimal result payloads
    public void SendMatchResultToPlatform(string outcome, int score = 0, int opponentScore = 0)
    {
        PostMatchResult(outcome, score, opponentScore);
    }

    // Editor/test helpers expected by ModeSwitcher
    //public void SwitchToAIMode()
    //{
    //    ResetGameMode();
    //    string json = "{\"matchId\":\"test_match\",\"playerId\":\"human_player\",\"opponentId\":\"a912345678\"}";
    //    InitParamsFromJS(json);
    //}

    public void SwitchToMultiplayerMode()
    {
        // In editor/testing mode, always use the test JSON from StartTestMatch regardless of external selection
#if UNITY_EDITOR || !UNITY_WEBGL
        Debug.Log("[IFrameBridge] External multiplayer selected, but using test JSON priority in editor mode");
        //StartTestMatch(); // This will use whatever JSON is currently uncommented
        return;
#else
		// On WebGL, external selection should work normally
		ResetGameMode();
		string json = "{\"matchId\":\"test_match\",\"playerId\":\"player1\",\"opponentId\":\"player2\",\"region\":\"in\"}";
		InitParamsFromJS(json);
#endif
    }

    [Serializable]
    private class MatchParams
    {
        public string matchId = string.Empty;
        public string playerId = string.Empty;
        public string opponentId = string.Empty;
        public string region = string.Empty;
    }
}

public enum GameType
{
    Singleplayer,
    Multiplayer,
}



