using Fusion;
using StarterAssets;
using System.Collections;
using UnityEngine;



public class GO_NetworkObject : NetworkBehaviour
{
    [Networked] public short level_ID { get; set; } = -1;
    
    [SerializeField] public bool IgnoreColisionChangeAuthority;
    [SerializeField] public bool IgnoreDisableOnChangeScene;

    public short levelindexLocal;
    
    public NetworkObject networkObject;
    public static bool readyChangeScene;
    public override void Spawned()
    {
        networkObject = GetComponent<NetworkObject>();
        if (Object.HasStateAuthority)
        {
            level_ID = (short)GO_SpawnPoint.currentSpawPoint.level_ID;
        }
        DontDestroyOnLoad(gameObject);
    }

    private IEnumerator Start()
    {
        yield return new WaitWhile(() => level_ID == -1);
        levelindexLocal = level_ID;
        yield return new WaitWhile(() => !GO_LevelManager.instance);
        yield return new WaitWhile(() => !GO_LevelManager.instance.isReadyObjects);
        yield return new WaitForSeconds(1);

        StartCoroutine(verifyLevelID());
        
    }
    private void OnEnable()
    {
        GO_LevelManager.OnPlayerChangeScene  += ChangeScene;
        GO_SpawnPoint.OnPlayerChangeSceneComplete  += ChangeSceneComplete;
    }

    private void OnDestroy()
    {        
        GO_LevelManager.OnPlayerChangeScene -= ChangeScene;
        GO_SpawnPoint.OnPlayerChangeSceneComplete -= ChangeSceneComplete;
    }

    private void ChangeScene()
    {
        Debug.Log("*-*MI NAME: " + name);
        Debug.Log("*-*MI NAME: " + Object.StateAuthority);
        Debug.Log("*-*MI NAME: " + GO_LevelManager.instance.CurrentPlayerRefChangeScene);

        if(GO_LevelManager.instance.CurrentPlayerRefChangeScene == Object.StateAuthority)
        {
            readyChangeScene = false;
            Debug.Log("---PASAR AUTORIDAD DE : " + name + " " + Object.StateAuthority + " - A este PLAYER REF" + GO_LevelManager.instance.newPlayerRefAuthorityChangeScene);
            RPC_RequestStateAuthority(GO_LevelManager.instance.newPlayerRefAuthorityChangeScene);
        }
       // RPC_RequestStateAuthority(Runner.GetPlayerObject(Object.StateAuthority));
    }
    private void ChangeSceneComplete()
    {
        Debug.Log("SE EJECTURÓ EL CHANGESCENE COMPLETE DEL NETWORKOBJECT");
        if (IgnoreDisableOnChangeScene)
        {
            return;
        }
        Debug.Log("---ACTIVAR EL PLAYER: " + name + " " + level_ID + " CONTRA: " + GO_SpawnPoint.currentSpawPoint.level_ID);
        gameObject.SetActive(levelindexLocal == (short)GO_SpawnPoint.currentSpawPoint.level_ID);
    }
    private IEnumerator verifyLevelID()
    {
        if (!IgnoreDisableOnChangeScene)
        {
            while (levelindexLocal == -1)
            {
                yield return null;
            }
            yield return null;
            Debug.Log("estado" + gameObject.activeSelf +" -- name " + gameObject.name + "LEVEL_INDEX" + level_ID);

            gameObject.SetActive(levelindexLocal == (short)GO_SpawnPoint.currentSpawPoint.level_ID);
            
        }
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
                ChangeAuthority();//;networkObject.RequestStateAuthority();
                //networkObject.AssignInputAuthority(Runner.LocalPlayer);
            }
            else
            {

            }
        }
    }

    public void ChangeAuthority()
    {
            networkObject.RequestStateAuthority();
    }

    public void Despawned()
    {
        ChangeAuthority();
        StartCoroutine(DespawnedCoroutine());
    }

    private IEnumerator DespawnedCoroutine()
    {
        yield return new WaitWhile(() => networkObject.StateAuthority != Runner.LocalPlayer);
        GO_LevelManager.instance.Despawned(Object);

    }

    //ARM Animation
    public void ShowGlow(bool _value)
    {
        transform.GetChild(0).gameObject.SetActive(_value);
        if (_value)
        {
            transform.rotation = Quaternion.identity;
        }
    }


}

