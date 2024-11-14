using Fusion;
using StarterAssets;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GO_PlayerNetworkManager : NetworkBehaviour
{

    private GO_ThirdPersonController controller;

    [Networked] public string playerName { get; set; }
    [Networked] public float playerLives { get; set; }
    [Networked] public short playerID { get; set; }
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
    private void Start()
    {
        (controller = GetComponentInChildren<GO_ThirdPersonController>()).enabled = isLocalPlayer;

    }

    public void TeleportPlayer(Vector3 _pos, Quaternion _rot)
    {
        Debug.Log("TELEPORT: " + playerTransform + " - " + _pos + " - " + _rot + " - " + playerTransform.name);
        playerTransform.Teleport(_pos, _rot);

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
