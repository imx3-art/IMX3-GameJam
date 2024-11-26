using StarterAssets;
using System.Collections;
using UnityEngine;

public class GO_PlayerActions : MonoBehaviour
{
    [SerializeField] private int maxPull = 100;
    [SerializeField] private float maxDistance = 10f;
    [SerializeField] private float maxUpDistance = 1.25f;
    [SerializeField] private GO_HudMiniGame hudMinigame;
    [SerializeField] private GO_PlayerNetworkManager otherPlayerNetworkManager;
    [SerializeField] private GO_PlayerNetworkManager otherPlayerNetworkManagerTMP;
    [SerializeField] private float timeMaxMiniGame = 5;

    [SerializeField] private GameObject leftArmPlayer;
    [SerializeField] private GameObject rightArmPlayer;
    [SerializeField] private LayerMask layerMask;
    [SerializeField] private GO_InputsPlayer _inputPlayer;

    private bool _endMiniGameCount = false;

    private Transform _rightHandRigg;
    private Transform _armTMP;
    private Transform _armExtraInRightHand; //brazo Extra
    private bool _gameEnd;

    private float _localPlayerRate = -1;
    private float _otherPlayerRate = -1;

    private Transform _target; // El objetivo al que quieres dirigir las manos
    private Animator _animator; // El componente Animator del personaje

    private float _fov;

    private void Start()
    {
        _inputPlayer = GetComponent<GO_InputsPlayer>();
        _animator = GetComponent<Animator>();
        _rightHandRigg = _animator.GetBoneTransform(HumanBodyBones.RightHand);
        _fov = GO_MainCamera.MainCamera.fieldOfView;
    }

    void Update()
    {
        
        if (_inputPlayer.drag && ReadyForMiniGame() > 0)
        {
            if (otherPlayerNetworkManager)
            {
                MiniGameDrag();
                ShakeCamera();
                if (GO_PlayerNetworkManager.localPlayer.isDrag == 1)
                {
                    return;
                }
            }
            
            Debug.DrawLine(transform.position + transform.up * maxUpDistance, transform.up * maxUpDistance + transform.position + transform.forward, Color.red);
            
            if (Physics.Raycast(transform.position + transform.up * maxUpDistance, transform.forward, out RaycastHit hitInfo, maxDistance, layerMask))
            {
                Debug.Log("***Objeto detectado: " + hitInfo.collider.gameObject.name);
                Debug.Log("***Distancia al objeto: " + hitInfo.distance);

                if (otherPlayerNetworkManager == null)
                {
                    otherPlayerNetworkManager = hitInfo.collider.gameObject.GetComponentInParent<GO_PlayerNetworkManager>();
                    if(otherPlayerNetworkManager.actionPlayer.ReadyForMiniGame() == 0)
                    {
                        otherPlayerNetworkManager = null;
                        return;
                    }
                    ResetMinigame();
                    _target = hitInfo.collider.transform;
                    GO_PlayerNetworkManager.localPlayer.otherPlayerTarget = otherPlayerNetworkManager;
                    GO_PlayerNetworkManager.localPlayer.StartdragMode(true);
                }
            }
            else
            {
                _target = null;
                otherPlayerNetworkManager = null;
                _inputPlayer.drag = false;
                DropArm(false, true);
            }
        }
        else if(GO_PlayerNetworkManager.localPlayer.isDrag == 2) //Control del otro player
        {
            hudMinigame.SetPullCount(GO_PlayerNetworkManager.localPlayer.pullMiniGame);
            hudMinigame.SetPullCount(GO_PlayerNetworkManager.localPlayer.otherPlayerTarget.pullMiniGame, false);
            ActiveCanvas(true);
            ShowTimeRemain();
        }
        else if(otherPlayerNetworkManager)
        {
            _target = null;
            otherPlayerNetworkManager = null;
        }
        else if (_inputPlayer.grabDropItem)
        {
            Debug.Log("RECOGIENO O TIRANDO");
            _inputPlayer.grabDropItem = false;
            if (!SetExtraArm(true))
            {
                BindUpArm();
            }
            else
            {
                Debug.Log("Ya tengo un brazo en la mano " + _armExtraInRightHand);
            }
        }
    }     
    
    private void MiniGameDrag()
    {
        if(_inputPlayer.pull && !_endMiniGameCount)
        {
            _inputPlayer.pull = false;
            hudMinigame.SetPullCount(GO_PlayerNetworkManager.localPlayer.pullMiniGame);
        }

        hudMinigame.SetPullCount(otherPlayerNetworkManager.pullMiniGame, false);
        
        if (GO_PlayerNetworkManager.localPlayer.pullMiniGame >= maxPull && !_endMiniGameCount)
        {
            _endMiniGameCount = true;        
            Debug.Log("*-*-*-GANADOR LOCAL " + GO_PlayerNetworkManager.localPlayer.timeMinigame + " CLICKS: " + GO_PlayerNetworkManager.localPlayer.pullMiniGame);
        }
        else if(otherPlayerNetworkManager.pullMiniGame >= maxPull && !_endMiniGameCount)
        {
            _endMiniGameCount = true;
            Debug.Log("*-*-*-GANADOR RIVAL" + GO_PlayerNetworkManager.localPlayer.timeMinigame + " CLICKS: " + otherPlayerNetworkManager.pullMiniGame);
        }        
        else if(_endMiniGameCount && !_gameEnd)
        {
            _gameEnd = true;
            EndMiniGame();
            ShowResult();
            Debug.Log("*-*-*-JUEGO FINALIZADO " + GO_PlayerNetworkManager.localPlayer.timeMinigame + " CLICKS: " + GO_PlayerNetworkManager.localPlayer.pullMiniGame + " vs " + otherPlayerNetworkManager.pullMiniGame);
        }

        if (!_endMiniGameCount)
        {
            ShowTimeRemain();
        }
    }
    private void ShowResult()
    {
        otherPlayerNetworkManagerTMP = otherPlayerNetworkManager;
        StartCoroutine(WaitResult());
    }
    private IEnumerator WaitResult()
    {
        _localPlayerRate = GO_PlayerNetworkManager.localPlayer.pullMiniGame / GO_PlayerNetworkManager.localPlayer.timeMinigame;

        yield return new WaitWhile(() => otherPlayerNetworkManagerTMP.isDrag > 0);

        _otherPlayerRate = otherPlayerNetworkManagerTMP.pullMiniGame / otherPlayerNetworkManagerTMP.timeMinigame;
                
        Debug.Log("+++ RESULTADO " + _localPlayerRate + " OTHER: " + _otherPlayerRate);
        if(_localPlayerRate < _otherPlayerRate)
        {
            Debug.Log("+++ GANO ENEMIGO ");
            GO_PlayerNetworkManager.localPlayer.RPC_setWinnerMiniGame(otherPlayerNetworkManagerTMP.playerID, GO_PlayerNetworkManager.localPlayer.playerID);
        }
        else
        {
            Debug.Log("+++ GANO LOCAL PLAYER");
            GO_PlayerNetworkManager.localPlayer.RPC_setWinnerMiniGame(GO_PlayerNetworkManager.localPlayer.playerID, otherPlayerNetworkManagerTMP.playerID);            
        }

    }

    public void SpawnArm(bool _spawAndDrag = true)
    {
        var armTMP = GO_LevelManager.instance.SpawnObjects(GO_LevelManager.instance.armPlayer.gameObject, _rightHandRigg.position, _rightHandRigg.rotation).transform;

        if (_spawAndDrag) 
        {
            _armExtraInRightHand = armTMP;
        }
    }

    private void EndMiniGame()
    {
        GO_PlayerNetworkManager.localPlayer.EnddragMode(true);
        ActiveCanvas(false);
        GO_PlayerNetworkManager.localPlayer.pullMiniGame = 0;
        _inputPlayer.drag = false;
        ShakeCamera(true);
    }

    public void ActiveCanvas(bool _state)
    {
        hudMinigame.gameObject.SetActive(_state);
    }
    public void ShowTimeRemain()
    {
        GO_PlayerNetworkManager.localPlayer.timeMinigame += Time.deltaTime;
        _endMiniGameCount = GO_PlayerNetworkManager.localPlayer.timeMinigame >= timeMaxMiniGame;
        hudMinigame.SetRemainTime(GO_PlayerNetworkManager.localPlayer.timeMinigame / timeMaxMiniGame);
    }    
    public void ResetMinigame(bool _canvas = true)
    {
        hudMinigame.ResetPullCount();
        hudMinigame.SetRemainTime(0);
        SetExtraArm(true);

        _endMiniGameCount = false;
        _gameEnd = false;
        GO_PlayerNetworkManager.localPlayer.timeMinigame = 0;
        GO_PlayerNetworkManager.localPlayer.pullMiniGame = 0;
        
        if (_canvas)
        {
            ActiveCanvas(true);
        }
    }

    private void OnAnimatorIK()
    {
        if (_target != null)
        {
            //Debug.Log("IK..." + animator + " - " + target);

            _animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 1);
            //animator.SetIKRotationWeight(AvatarIKGoal.RightHand, 1);
            _animator.SetIKPosition(AvatarIKGoal.RightHand, _target.position);
            //animator.SetIKRotation(AvatarIKGoal.RightHand, Quaternion.LookRotation(target.position - transform.position));
            
            _animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1);
            //animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 1);
            _animator.SetIKPosition(AvatarIKGoal.LeftHand, _target.position);
            //animator.SetIKRotation(AvatarIKGoal.LeftHand, Quaternion.LookRotation(target.position - transform.position));
        }
        else
        {
            _animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 0);
            //animator.SetIKRotationWeight(AvatarIKGoal.RightHand, 0);
            _animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 0);
            //animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 0);
        }
    }
    private void LateUpdate()
    {
        hudMinigame.transform.LookAt(GO_MainCamera.MainCamera.transform);
        if (_armExtraInRightHand)
        {
            _armExtraInRightHand.position = _rightHandRigg.position;//, rightHand.rotation);
            _armExtraInRightHand.rotation = _rightHandRigg.rotation;
        }
    }
    public void DropArm(bool _State, bool _self = false)
    {
        if( _self && _armExtraInRightHand) 
        {
        _armExtraInRightHand = null;
        return;
        }

        if((rightArmPlayer.activeSelf && !_State) || (!rightArmPlayer.activeSelf && _State) )
        {
            rightArmPlayer.SetActive(_State);
            if(_self ) 
            { 
            SpawnArm(false);    
            }
            return;
        }
        if((leftArmPlayer.activeSelf && !_State) || (!leftArmPlayer.activeSelf && _State))
        {
            leftArmPlayer.SetActive(_State);
            if (_self)
            {
                SpawnArm(false);
            }
            return;
        }
    }

    public bool SetExtraArm(bool _drop = false)
    {
        if(_armExtraInRightHand)
        {
            if(_drop)
            {
                _armExtraInRightHand = null;
            }
            return true;
        }
        return false;
    }

    private int ReadyForMiniGame()
    {
        return (leftArmPlayer.activeSelf ? 1 : 0) + (rightArmPlayer.activeSelf ? 1 : 0);
    }
        
    public void ShakeCamera(bool _value = false)
    {
        GO_MainCamera.cinemachineBrain.enabled = _value || _gameEnd;
        GO_MainCamera.MainCamera.fieldOfView = Mathf.Lerp(GO_MainCamera.MainCamera.fieldOfView, Random.Range(_fov * .7f, _fov * 1.3f), Time.deltaTime * 4);
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("DETECTO ITEM: " + other.name + " - " + other.gameObject.layer);

        if(other.gameObject.layer == 8)
        {
            _armTMP = other.transform;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Debug.Log("ALEJO ITEM: " + other.name + " - " + other.gameObject.layer);

        if(other.gameObject.layer == 8 && _armTMP == other.transform)
        {
            _armTMP = null;
        }
    }

    private void BindUpArm()
    {
        if (!_armTMP)
        {
            return;
        }
        Debug.Log("AGARRO ITEM: " + _armTMP.name);
        _armTMP.GetComponent<GO_NetworkObject>().ChangeAuthority();

        if (ReadyForMiniGame() == 2)
        {
            _armExtraInRightHand = _armTMP;
        }
        else
        {
            _armTMP.GetComponent<GO_NetworkObject>().Despawned();
            GO_PlayerNetworkManager.localPlayer.RPC_addNewArm();
        }
        _armTMP = null;
    }
}