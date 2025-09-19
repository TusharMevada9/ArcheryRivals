using Fusion;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;
using static Unity.Collections.Unicode;

public class ArrowCollisionMultiplayer : NetworkBehaviour
{
    [Header("Collision Settings")]
    public string targetTag = "Target";      
    public bool stopOnCollision = true;        
    public bool destroyOnCollision = false; 
    public bool makeTargetKinematic = true; 

    [Header("Visual Effects")]
    public bool showHitEffect = true;      
    public Color hitColor = Color.red;
    private Rigidbody2D arrowRb;
    private SpriteRenderer arrowSprite;

    public NetworkObject SpawnArrow;


    public NetworkObject HalfArrow;

    public int Count = 0;
    void Start()
    {
        arrowRb = GetComponent<Rigidbody2D>();
        arrowSprite = GetComponent<SpriteRenderer>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag(targetTag))
        {
            HandleCollision2D(other);
        }
    }

    public void HandleCollision2D(Collider2D targetCollider)
    {
        Debug.LogError("Arrow hit target - will not be destroyed");

        if (HalfArrow != null) 
        {
            FusionConnector.instance.NetworkRunner.Despawn(HalfArrow);
       
        }


        if (targetCollider.CompareTag("Red"))
        {
            RPCRedScore();
        }
        else if (targetCollider.CompareTag("Blue"))
        {
            RPCBlueScore();
        }

        NetworkObject New = FusionConnector.instance.NetworkRunner.Spawn(SpawnArrow);
        HalfArrow = New;
        RPCTrueHalf(New);


        Vector2 Pos = targetCollider.transform.position;
        Pos.x -= 0.3f;

        RPCPosHalf(Pos);
        FusionConnector.instance.NetworkRunner.Despawn(targetCollider.GetComponent<NetworkObject>());
    }


    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPCTrueHalf(NetworkObject New)
    {
        HalfArrow = New;
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPCRedScore()
    {
        UIManager.Instance.redScore += 1;
        UIManager.Instance.redScoreText.text = "Red Score: " + UIManager.Instance.redScore.ToString();
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPCBlueScore()
    {
        UIManager.Instance.blueScore += 1;
        UIManager.Instance.blueScoreText.text = "Blue Score: " + UIManager.Instance.redScore.ToString();
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPCPosHalf(Vector2 pos)
    {
        HalfArrow.transform.localPosition = pos;
        HalfArrow.transform.SetParent(this.gameObject.transform);
    }
}
