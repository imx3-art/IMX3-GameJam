using Fusion;
using StarterAssets;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;



public class GO_NetworkObject : NetworkBehaviour
{
    [Networked] public short levelID { get; set; }
    
    [SerializeField] private bool IgnoreColisionChangeAuthority;
    
    private NetworkObject networkObject;
    public static bool readyChangeScene;
    private void Start()
    {
        Debug.Log("Start " + name);
        networkObject = GetComponent<NetworkObject>();
        levelID = GO_SpawnPoint.currentSpawPoint.level_ID;
        DontDestroyOnLoad(gameObject);

    }
    private void OnEnable()
    {
        GO_LevelManager.OnPlayerChangeScene  += ChangeScene;
    }

    private void OnDisable()
    {
        GO_LevelManager.OnPlayerChangeScene -= ChangeScene;
    }

    private void ChangeScene()
    {
        if(GO_LevelManager.instance.CurrentPlayerRefChangeScene == Object.StateAuthority)
        {
            readyChangeScene = false;
            Debug.Log("---PASAR AUTORIDAD DE : " + name + " " + Object.StateAuthority + " - A este PLAYER REF" + GO_LevelManager.instance.newPlayerRefAuthorityChangeScene);
            RPC_RequestStateAuthority(GO_LevelManager.instance.newPlayerRefAuthorityChangeScene);
        }
       // RPC_RequestStateAuthority(Runner.GetPlayerObject(Object.StateAuthority));
    }


    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RPC_RequestStateAuthority(PlayerRef _player)
    {
        Debug.LogFormat("---RECIBIDO " + _player + " REQUIRIENDO AUTORIDAD DE: " + name);        
        if(_player == Runner.LocalPlayer)
        {            
            Object.RequestStateAuthority();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(IgnoreColisionChangeAuthority)
        {
            return; 
        }
        GO_ThirdPersonController playerNetworkManager;
        Debug.Log("COLISION PLAYER: " + collision.gameObject.name);
        if (collision.gameObject.TryGetComponent(out playerNetworkManager))
        {
            Debug.Log("COLISION PLAYER: " + playerNetworkManager);
            if (playerNetworkManager.enabled && !networkObject.HasStateAuthority)
            {
                networkObject.RequestStateAuthority();
                //networkObject.AssignInputAuthority(Runner.LocalPlayer);
            }
            else
            {

            }
        }


    }




}

