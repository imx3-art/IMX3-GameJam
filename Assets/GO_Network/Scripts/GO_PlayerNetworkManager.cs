using System;
using Fusion;
using StarterAssets;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;
using UnityEngine.InputSystem;

public enum PlayerState
{
    Normal,
    Persecution,
    Duel,
    Ghost
}

public class GO_PlayerNetworkManager : NetworkBehaviour
{

    private GO_ThirdPersonController controller;

    [Networked] public string playerName { get; set; }
    [Networked] public short idColor { get; set; } = -1;
    [Networked] public float playerLives { get; set; }
    [Networked] public short playerID { get; set; }
    [Networked] public short currentLevel_ID { get; set; }
    [Networked] public short isDrag { get; set; }
    [Networked] public short pullMiniGame { get; set; }
    [Networked] public float timeMinigame { get; set; }
    [SerializeField] private GameObject cameraTargetFollow;

    [SerializeField] private GO_PlayerUIManager canvasUIPlayer;
    [SerializeField] private Renderer colorPlayer;

    public NetworkTransform playerTransform;
    public GO_InputsPlayer inputPlayer;

    public bool isLocalPlayer;
    public static List<GO_PlayerNetworkManager> PlayersList = new List<GO_PlayerNetworkManager>();
    public static GO_PlayerNetworkManager localPlayer;
    public GO_PlayerNetworkManager otherPlayerTarget;
    public GO_PlayerActions actionPlayer;
    public PlayerState CurrentPlayerState;

    public event Action<PlayerState> OnPlayerStateChanged; 
    
    public override void Spawned()
    {
        DontDestroyOnLoad(gameObject);
        if (Object.HasStateAuthority)
        {
            GetComponentInChildren<PlayerInput>().enabled = true;
            GetComponentInChildren<GO_PlayerActions>().enabled = true;
            GetComponent<GO_FadeObjectBlockingObject>().enabled = true;
            localPlayer = this;
            isLocalPlayer = true;
            playerTransform.gameObject.transform.localPosition = Vector3.zero;
            playerID = (short)Random.Range(1000, 9999);
            CurrentPlayerState = PlayerState.Normal;
            inputPlayer = GetComponentInChildren<GO_InputsPlayer>();
            Instantiate(canvasUIPlayer.gameObject, transform);
        }
        else
        {
            Destroy(GetComponentInChildren<GO_InputsPlayer>());
        }
        actionPlayer = GetComponentInChildren<GO_PlayerActions>();
        cameraTargetFollow.SetActive(isLocalPlayer);

        if (isLocalPlayer)
        {
            OnPlayerStateChanged += HandlePlayerStateChanged;
        }
        PlayersList.Add(this);
        GetColorPlayer();
    }
    public void GetColorPlayer()
    {
        if (PlayersList.Count != Runner.SessionInfo.PlayerCount ||
            !GO_LevelManager.instance)
        {
            Invoke("GetColorPlayer", 1);
            return;
        }
        if (Object.HasStateAuthority)
        {
            for (int i = 0; i < GO_LevelManager.instance.playerColors.Length; i++)
            {
                bool isUse = false;
                Debug.Log("Ccomparo : " + i);

                for (int j = 0; j < PlayersList.Count; j++)
                {
                    Debug.Log("Ccomparo player : " + PlayersList[j].idColor);
                    if (PlayersList[j].idColor == i)
                    {
                        isUse = true;
                        break;
                    }
                }
                if (!isUse)
                {
                    idColor = (short) i;
                    break;
                }
            }
        }
        SetColorPlayer();
    }

    private void SetColorPlayer()
    {
        if (idColor < 0)
        {
            Invoke("SetColorPlayer", 1);
            return;
        }
        //colorPlayer.material.color = GO_LevelManager.instance.playerColors[idColor];
        colorPlayer.material.SetColor("_ReflectionColor", GO_LevelManager.instance.playerColors[idColor]);
    }
    private IEnumerator Start()
    {
        (controller = GetComponentInChildren<GO_ThirdPersonController>()).enabled = isLocalPlayer;
        yield return new WaitWhile(() => GO_LevelManager.instance == null); 
        while (!GO_LevelManager.instance.isReady) 
        {
        yield return null;
        }
        GO_LevelManager.instance.CheckPlayerInNewLevel();        
    }
    private void OnDisable()
    {
        PlayersList.Remove(this);

    }
    
    private void OnDestroy()
    {
        if (isLocalPlayer)
        {
            OnPlayerStateChanged -= HandlePlayerStateChanged;
        }
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
            if((playerTransform.transform.position - _pos).magnitude < 4f)
            {
                break;
            }
        }
        while (true);// (playerTransform.transform.position - _pos).magnitude < .5f);

        if (GO_LoadScene.Instance)
        {
            GO_LoadScene.Instance.HideLoadingScreen();
        }
        ChangePlayerState(PlayerState.Normal);
        RPC_setState((int)PlayerState.Normal);
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
        GO_MainCamera.cinemachineBrain.enabled = true;

        if (_RPC && otherPlayerTarget != null)
        {
            RPC_setOtherPlayer(otherPlayerTarget.playerID, playerID, false);
        }
        otherPlayerTarget = null;
        actionPlayer.ActiveCanvas(false);
    }
    
    public void ChangePlayerState(PlayerState newState)
    {
        CurrentPlayerState = newState;
        OnPlayerStateChanged?.Invoke(CurrentPlayerState);
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
                otherPlayerTargetTMP.isDrag = 2;
                otherPlayerTargetTMP.pullMiniGame = 0;
                otherPlayerTargetTMP.timeMinigame = 0;
                otherPlayerTargetTMP.actionPlayer.ResetMinigame(false);
                //otherPlayerTargetTMP.StartdragMode();
            }
            else
            {
                otherPlayerTargetTMP.EnddragMode();
            }
            Debug.Log("*****EL RPC FUNCIONA SOY: " + _playerID + " - Busco a: " + _otherPlayer + " - otherPlayer: " + otherPlayerTarget);
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)] 
    public void RPC_setState(int state)
    {
        ChangePlayerState((PlayerState)state);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]//, HostMode = RpcHostMode.SourceIsHostPlayer)]
    public void RPC_setWinnerMiniGame(short _playerWinID, short _playerLoseID)
    {
        GO_PlayerNetworkManager playerWin = PlayersList.Find(player => player.playerID == _playerWinID);
        GO_PlayerNetworkManager playerLose = PlayersList.Find(player => player.playerID == _playerLoseID);
        {
            playerWin.DragArm(playerWin);
            playerLose.DropArm();
        }

    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]//, HostMode = RpcHostMode.SourceIsHostPlayer)]
    public void RPC_addNewArm()
    {
        actionPlayer.DropArm(true);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]//, HostMode = RpcHostMode.SourceIsHostPlayer)]
    public void RPC_SelfDropArm()
    {
        actionPlayer.DropArm(false, true);
    }
    
    public void DropArm()
    {
        Debug.Log("LOSE ARM");
        actionPlayer.DropArm(false);
    }

    public void DragArm(GO_PlayerNetworkManager _playerWin)
    {
        if (actionPlayer.ReadyForMiniGame() < 2)
        {
            actionPlayer.DropArm(true);
        }
        else
        {
            if (_playerWin.Equals(localPlayer))
            {
                actionPlayer.SpawnArm();
            }
        }
        actionPlayer.UpdateArms();


    }

    private void HandlePlayerStateChanged(PlayerState newState)
    {
        switch (newState)
        {
            case PlayerState.Normal:
                GO_AudioManager.Instance.PlayAmbientSound("NormalStateSound");
                break;
            case PlayerState.Persecution:
                GO_AudioManager.Instance.PlayAmbientSound("PersecutionStateSound");
                controller.HandleCameraDistance(controller.persecutionCameraDistance);
                break;
            default:
                GO_AudioManager.Instance.StopAmbientSound();
                break;
        }
    }
}
