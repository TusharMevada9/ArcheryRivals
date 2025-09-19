using Fusion;
using UnityEngine;

public class RPCManager : MonoBehaviour
{
    public static RPCManager Instance;

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


    //[Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    //public void RPCSetParent(NetworkObject New,Transform This)
    //{
    //    New.transform.SetParent(This);
    //}


    //[Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    //public void RPCSetHalfArrowPos(NetworkObject Obj, Vector2 pos)
    //{
    //    Obj.transform.position = pos;
    //}
}
