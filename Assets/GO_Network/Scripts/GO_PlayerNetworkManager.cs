using Fusion;
using StarterAssets;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class GO_PlayerNetworkManager : NetworkBehaviour
{

    private GO_ThirdPersonController controller;

    [Networked] public string playerName { get; set; }
    [Networked] public float playerLives { get; set; }
    [Networked] public short playerID { get; set; }
    [Networked] public short currentLevel_ID { get; set; }
    [Networked] public Vector2 movePlayerNetwork { get; set; }
    [Networked] public short isDrag { get; set; }

    [SerializeField] private GameObject cameraTargetFollow;

    public NetworkTransform playerTransform;
    public bool isLocalPlayer;
    public static List<GO_PlayerNetworkManager> PlayersList = new List<GO_PlayerNetworkManager>();
    public static GO_PlayerNetworkManager localPlayer;
    public GO_PlayerNetworkManager otherPlayerTarget;

    public override void Spawned()
    {
        PlayersList.Add(this);
        DontDestroyOnLoad(gameObject);
        if (Object.HasStateAuthority)
        {
            localPlayer = this;
            isLocalPlayer = true;
            playerTransform.gameObject.transform.localPosition = Vector3.zero;
            playerID = (short)Random.Range(1000, 9999);
        }
        else
        {

        }
        cameraTargetFollow.SetActive(isLocalPlayer);

        //GetComponentInChildren<Rigidbody>().isKinematic = !isLocalPlayer;
        //(controller = GetComponentInChildren<GO_ThirdPersonController>()).enabled = isLocalPlayer;

    }
    private IEnumerator Start()
    {
        (controller = GetComponentInChildren<GO_ThirdPersonController>()).enabled = isLocalPlayer;
        yield return new WaitWhile(() => GO_LevelManager.instance == null); 
        while (!GO_LevelManager.instance.isReady) 
        {
        yield return null;
        }
        GO_LevelManager.instance.StatusScene();
    }
    private void OnDisable()
    {
        PlayersList.Remove(this);

    }

    public void TeleportPlayer(Vector3 _pos, Quaternion _rot)
    {
        StartCoroutine(TeleportPlayerCoroutine(_pos, _rot));
    }
    private IEnumerator TeleportPlayerCoroutine(Vector3 _pos, Quaternion _rot)
    {
        currentLevel_ID = (short) GO_SpawnPoint.currentSpawPoint.level_ID;
        do
        {
            playerTransform.Teleport(_pos, _rot);
            yield return new WaitForSeconds(1);
            Debug.Log("TELEPORT: " + playerTransform.transform.position + " - " + _pos + " - " + (playerTransform.transform.position - _pos).magnitude);
            if((playerTransform.transform.position - _pos).magnitude < 2f)
            {
                break;
            }
        }
        while (true);// (playerTransform.transform.position - _pos).magnitude < .5f);
        //QUITAR LA PANTALLA DE CARGA

    }

    public void StartdragMode(bool _RPC = false)
    {
        isDrag = 1;
        if (_RPC)
        {
            RPC_setOtherPlayer(otherPlayerTarget.playerID, playerID, true);
        }
    }
    public void EnddragMode(bool _RPC = false)
    {
        isDrag = 0;

        if (_RPC && otherPlayerTarget != null)
        {
            RPC_setOtherPlayer(otherPlayerTarget.playerID, playerID, false);
        }
        otherPlayerTarget = null;
    }


    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]//, HostMode = RpcHostMode.SourceIsHostPlayer)]
    public void RPC_setOtherPlayer(short _playerID, short _otherPlayer, bool start)
    {
        GO_PlayerNetworkManager otherPlayerTargetTMP = PlayersList.Find(player => player.playerID == _playerID);
        if (otherPlayerTargetTMP)
        {
            if (start)
            {
                otherPlayerTargetTMP.otherPlayerTarget = PlayersList.Find(player => player.playerID == _otherPlayer);
                otherPlayerTargetTMP.StartdragMode();
            }
            else
            {
                otherPlayerTargetTMP.EnddragMode();
            }
            Debug.Log("*****EL RPC FUNCIONA SOY: " + _playerID + " - Busco a: " + _otherPlayer + " - otherPlayer: " + otherPlayerTarget);
        }
    }

}
