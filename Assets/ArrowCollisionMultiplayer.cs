using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.ParticleSystem;

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

    public GameObject Particals;

    public int Count = 0;
    void Start()
    {
        arrowRb = GetComponent<Rigidbody2D>();
        arrowSprite = GetComponent<SpriteRenderer>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (Object.HasStateAuthority)
        {
            if (UIManager.Instance.isGameStart == true)
            {
                if (other.gameObject.CompareTag(targetTag))
                {
                    HandleCollision2D(other);
                }
            }
        }
    }

    public void HandleCollision2D(Collider2D targetCollider)
    {
        Debug.LogError("Arrow hit target - will not be destroyed");

        // Play arrow hit target SFX
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayRandomArrowHitTarget();
        }

        if (HalfArrow != null) 
        {
            FusionConnector.instance.NetworkRunner.Despawn(HalfArrow);
       
        }
        RPCParticalTrue();

        if (targetCollider.CompareTag("Red"))
        {
            RPCRedScore();
        }
        else if (targetCollider.CompareTag("Blue"))
        {
            RPCBlueScore();
        }

       


        Vector2 Pos = targetCollider.transform.position;
        //Pos.x -= 0.3f;
        FusionConnector.instance.NetworkRunner.Despawn(targetCollider.GetComponent<NetworkObject>());

        //yield return new WaitForSeconds(0.01f);
        NetworkObject New = FusionConnector.instance.NetworkRunner.Spawn(SpawnArrow);
        HalfArrow = New;
        RPCTrueHalf(New);
        RPCPosHalf(Pos);

        //yield return new WaitForSeconds(1f);
       Invoke(nameof(RPCParticalFalse),1f);
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
        UIManager.Instance.redScoreText.text =  UIManager.Instance.redScore.ToString();
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPCBlueScore()
    {
        UIManager.Instance.blueScore += 1;
        UIManager.Instance.blueScoreText.text = UIManager.Instance.blueScore.ToString();
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPCPosHalf(Vector2 pos)
    {
        HalfArrow.transform.localPosition = pos;
        HalfArrow.transform.SetParent(this.gameObject.transform);

        if (targetTag == "Red")
        {
            HalfArrow.transform.localPosition = new Vector2(-0.38f, HalfArrow.transform.localPosition.y);
        }
        else
        {
            HalfArrow.transform.localPosition = new Vector2(0.38f, HalfArrow.transform.localPosition.y);

        }
    }


    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPCParticalTrue()
    {
        Particals.SetActive(true);
        Particals.GetComponent<ParticleSystem>().Play();
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPCParticalFalse()
    {
        Particals.SetActive(false);
        Particals.GetComponent<ParticleSystem>().Stop();
    }


    //private void Update()
    //{
    //    if (Object.HasInputAuthority)
    //    {
    //        RPC_UpdatePosition(this.transform.position);
    //    }

    //    Debug.Log("Enter");
    //}

    //[Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    //public void RPC_UpdatePosition(Vector3 newPos)
    //{
    //    // Server / StateAuthority side update
    //    transform.position = newPos;
    //}
}
