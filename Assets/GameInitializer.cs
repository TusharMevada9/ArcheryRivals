using UnityEngine;
using System.Collections;
using Fusion;
using System.Collections.Generic;

public class GameInitializer : NetworkBehaviour
{
    public static GameInitializer Instance { get; private set; }

    [Header("PlayerPrefab Multiplayer")]
    public NetworkObject RedBow;
    public NetworkObject BlueBow;

    [Space(30)]
    [Header("TargetPrefabMultiplayer")]
    public NetworkObject RedTarget;
    public NetworkObject BlueTarget;

    [Header("SinglePlayerPrefab")]
    public GameObject SingleRedBow;
    public GameObject SingleBlueBow;

    [Space(30)]
    [Header("SingleTargetPrefabMultiplayer")]
    public GameObject SingleRedTarget;
    public GameObject SingleBlueTarget;

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

    }

    public void InitializeMatch()
    {
        if (IFrameBridge.Instance != null && IFrameBridge.Instance.gameType == GameType.Singleplayer)
        {
            InitializeAIMode();
        }
        else
        {
            InitializeMultiplayerMode();
        }
    }

    private void InitializeAIMode()
    {
        GameManager.Instance.AiModeSpawnPlayer();
    }

    private void InitializeMultiplayerMode()
    {
       
    }

}
