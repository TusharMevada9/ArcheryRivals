using UnityEngine;
using System.Collections.Generic;
using Fusion;
using System.Collections;
using System.Linq;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public Transform RedBowspawn;
    public Transform BlueBowspawn;

    [Header("TargetPoint")]
    public Transform RedTargetPoint;
    public Transform BlueTargetPoint;


    public int playerID;
    public ArrowShooterMultiPlayer playerCarBehaviour;
    public ArrowShooterMultiPlayer opponentCarBehaviour;

    void Awake()
    {
        // Singleton pattern - ensure only one instance exists
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

    public void AiModeSpawnPlayer()
    {
        Instantiate(GameInitializer.Instance.SingleRedBow, RedBowspawn.position,Quaternion.identity);
        Instantiate(GameInitializer.Instance.SingleRedTarget, RedTargetPoint.position,Quaternion.identity);



        Instantiate(GameInitializer.Instance.SingleBlueBow, BlueBowspawn.position,Quaternion.Euler(0,0,180));
        Instantiate(GameInitializer.Instance.SingleBlueTarget, RedTargetPoint.position, Quaternion.identity);
    }


    public IEnumerator SpawnPlayer(NetworkRunner runner)
    {

        Debug.Log("Runner Local Player Id");

        if (runner.LocalPlayer.PlayerId == 1)
        {
            Debug.Log("[GameManager] Spawning Player 1 (Local Player) at player spawn point");

           // UIManager.instance.YOUText.text = "You";
            //UIManager.instance.OpponentText.text = "Opponent";

            NetworkObject playerObj = runner.Spawn(GameInitializer.Instance.RedBow, RedBowspawn.position, Quaternion.identity);

            runner.Spawn(GameInitializer.Instance.RedTarget, RedTargetPoint.position, Quaternion.identity);
            playerID = 1;

            playerCarBehaviour = playerObj.GetComponent<ArrowShooterMultiPlayer>();
          
        }
        else
        {
            Debug.Log("[GameManager] Spawning Player 2 (Opponent) at AI spawn point");
          
           // UIManager.instance.YOUText.text = "Opponent";
            //UIManager.instance.OpponentText.text = "You";

            NetworkObject playerObj = runner.Spawn(GameInitializer.Instance.BlueBow, BlueBowspawn.position, Quaternion.Euler(0,0,180));
            runner.Spawn(GameInitializer.Instance.BlueTarget, BlueTargetPoint.position, Quaternion.identity);

            playerID = 2;
            opponentCarBehaviour = playerObj.GetComponent<ArrowShooterMultiPlayer>();

        }

        if (runner.ActivePlayers.Count() >= 2)
        {
            Debug.Log("[GameManager] Both players joined - waiting for countdown to finish");
        }

        yield return new WaitForSeconds(0.1f);
    }

}
