using Fusion;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using Fusion.Sockets;
using System.Linq;

public class FusionConnector : MonoBehaviour, INetworkRunnerCallbacks
{
    public static FusionConnector instance;

    [SerializeField]
    private NetworkRunner networkRunner;

    public NetworkRunner NetworkRunner => networkRunner;

    private void Awake()
    {
        instance = this;
    }

    internal async void ConnectToServer(string sessionName)
    {
        if (networkRunner == null)
            networkRunner = gameObject.AddComponent<NetworkRunner>();

        networkRunner.ProvideInput = true;

        var result = await networkRunner.StartGame(
            new StartGameArgs()
            {
                GameMode = GameMode.Shared,
                SessionName = sessionName,
                PlayerCount = 2, 
            }
        );

        if (result.Ok)
        {
            networkRunner.AddCallbacks(this);

            StartCoroutine(GameManager.Instance.SpawnPlayer(networkRunner));
        }
    }

    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        Debug.Log($"[FusionConnector] 🎯 Player joined: PlayerRef {player}, Total players: {runner.ActivePlayers.Count()}");

        // 🔧 SOLUTION 1: Better Client ID Tracking
        foreach (var activePlayer in runner.ActivePlayers)
        {
            Debug.Log($"[FusionConnector] 👥 Active Player: {activePlayer} (Client ID: {activePlayer.RawEncoded})");
        }

        // Check if this is the local player
        if (player == runner.LocalPlayer)
        {
            Debug.Log($"[FusionConnector] 🚀 I am PlayerRef {player} - Local Player ID: {runner.LocalPlayer}");
        }
        else
        {
            Debug.Log($"[FusionConnector] 👤 Opponent joined: PlayerRef {player}");
        }

        if (runner.ActivePlayers.Count() >= 2)
        {
            Debug.Log("🎮 2 players joined → Starting countdown!");
            // Start the countdown when both players are ready
            //if (GameManager.Instance != null)
            //{
            //    GameManager.Instance.RPC_StartCountdown();
            //}
        }
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        bool isOpponent = player != runner.LocalPlayer;

        //if (isOpponent)
        //{
        //    Debug.Log($"[FusionConnector] Opponent {player} left the game - triggering forfeit");
        //    if (IFrameBridge.Instance != null)
        //    {
        //        IFrameBridge.Instance.PostOpponentForfeit(IFrameBridge.OpponentId);
        //    }
        //    // End match locally showing a win for the remaining player
        //    var ui = FindFirstObjectByType<UIManager>();
        //    if (ui != null)
        //    {
        //        ui.EndMatchForForfeit(true);
        //    }
        //}
        //else
        //{
        //    Debug.Log($"[FusionConnector] Local player {player} left the game");
        //    if (IFrameBridge.Instance != null)
        //    {
        //        IFrameBridge.Instance.PostPlayerForfeit();
        //    }
        //    // End match locally showing a loss for the leaver
        //    var ui = FindFirstObjectByType<UIManager>();
        //    if (ui != null)
        //    {
        //        ui.EndMatchForForfeit(false);
        //    }
        //}
    }

    // Implement other INetworkRunnerCallbacks as empty or with basic logging
    public void OnConnectedToServer(NetworkRunner runner) { Debug.Log("Connected to server"); }
    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { Debug.Log($"Connect failed: {reason}"); }
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
    {
        Debug.Log($"[Connector] Disconnected from server: {reason}");
    }
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) { }
    public void OnSceneLoadDone(NetworkRunner runner) { }
    public void OnSceneLoadStart(NetworkRunner runner) { }
    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { Debug.Log($"Shutdown: {shutdownReason}"); }
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
}
