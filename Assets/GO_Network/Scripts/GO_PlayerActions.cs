using StarterAssets;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class GO_PlayerActions : MonoBehaviour
{
    [SerializeField] private int maxPull = 100;
    [SerializeField] private float maxDistance = 10f;
    [SerializeField] private float maxUpDistance = 1.25f;
    [SerializeField] private GO_HudMiniGame hudMinigame;
    [SerializeField] private GO_PlayerNetworkManager otherPlayerNetworkManager;
    [SerializeField] private GO_PlayerNetworkManager otherPlayerNetworkManagerTMP;
    //[SerializeField] private float timeCurrentMiniGameCount = 0;
    [SerializeField] private float timeMaxMiniGame = 5;
    [SerializeField] private bool endMiniGameCount = false;

    [SerializeField] private GameObject leftArmPlayer;
    [SerializeField] private GameObject rightArmPlayer;

    private Transform rightHandRigg;
    private Transform armInRightHand;

    private float localPlayerRate = -1;
    private float otherPlayerRate = -1;

    public LayerMask layerMask;
    GO_InputsPlayer inputPlayer;

    private void Start()
    {
        inputPlayer = GetComponent<GO_InputsPlayer>();
        animator = GetComponent<Animator>();
        rightHandRigg = animator.GetBoneTransform(HumanBodyBones.RightHand);
    }

    void Update()
    {

        if (inputPlayer.drag && ReadyForMiniGame())
        {
            if (otherPlayerNetworkManager)
            {
                MiniGameDrag();
                if(GO_PlayerNetworkManager.localPlayer.isDrag == 1)
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
                    ResetMinigame();
                    target = hitInfo.collider.transform;
                    otherPlayerNetworkManager = hitInfo.collider.gameObject.GetComponentInParent<GO_PlayerNetworkManager>();
                    GO_PlayerNetworkManager.localPlayer.otherPlayerTarget = otherPlayerNetworkManager;
                    GO_PlayerNetworkManager.localPlayer.StartdragMode(true);
                }
            }
            else
            {
                target = null;
                otherPlayerNetworkManager = null;
                inputPlayer.drag = false;
            }
        }
        else if(GO_PlayerNetworkManager.localPlayer.isDrag == 2) //Control del otro player
        {
            //timeCurrentMiniGameCount += Time.deltaTime;
            GO_PlayerNetworkManager.localPlayer.timeMinigame += Time.deltaTime;
            hudMinigame.SetPullCount(GO_PlayerNetworkManager.localPlayer.pullMiniGame);
            hudMinigame.SetPullCount(GO_PlayerNetworkManager.localPlayer.otherPlayerTarget.pullMiniGame, false);
            ActiveCanvas(true);
            ShowTimeRemain();
        }
        else if(otherPlayerNetworkManager)
        {
            target = null;
            otherPlayerNetworkManager = null;
        }
        else if(armInRightHand)
        {
            armInRightHand.position = rightHandRigg.position;//, rightHand.rotation);
            armInRightHand.rotation = rightHandRigg.rotation;
        }
    }

    private bool gameEnd;
    private float inc = .1f;
    private void MiniGameDrag()
    {
        if(inputPlayer.pull && !endMiniGameCount)
        {
            inputPlayer.pull = false;
            hudMinigame.SetPullCount(GO_PlayerNetworkManager.localPlayer.pullMiniGame);
        }

        hudMinigame.SetPullCount(otherPlayerNetworkManager.pullMiniGame, false);


        if (GO_PlayerNetworkManager.localPlayer.pullMiniGame >= maxPull && !endMiniGameCount)
        {
            endMiniGameCount = true;
            //ShowResult();
            
            Debug.Log("*-*-*-GANADOR LOCAL " + GO_PlayerNetworkManager.localPlayer.timeMinigame + " CLICKS: " + GO_PlayerNetworkManager.localPlayer.pullMiniGame);
        }
        else if(otherPlayerNetworkManager.pullMiniGame >= maxPull && !endMiniGameCount)
        {
            endMiniGameCount = true;
            //ShowResult();
            Debug.Log("*-*-*-GANADOR RIVAL" + GO_PlayerNetworkManager.localPlayer.timeMinigame + " CLICKS: " + otherPlayerNetworkManager.pullMiniGame);
        }
        
        else if(endMiniGameCount && !gameEnd)
        {
            gameEnd = true;
            EndMiniGame();
            ShowResult();
            Debug.Log("*-*-*-JUEGO FINALIZADO " + GO_PlayerNetworkManager.localPlayer.timeMinigame + " CLICKS: " + GO_PlayerNetworkManager.localPlayer.pullMiniGame + " vs " + otherPlayerNetworkManager.pullMiniGame);
        }
        if (!endMiniGameCount)
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
        localPlayerRate = GO_PlayerNetworkManager.localPlayer.pullMiniGame / GO_PlayerNetworkManager.localPlayer.timeMinigame;

        yield return new WaitWhile(() => otherPlayerNetworkManagerTMP.isDrag > 0);

        otherPlayerRate = otherPlayerNetworkManagerTMP.pullMiniGame / otherPlayerNetworkManagerTMP.timeMinigame;
                
        Debug.Log("+++ RESULTADO " + localPlayerRate + " OTHER: " + otherPlayerRate);
        if(localPlayerRate < otherPlayerRate)
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

    public void SpawnArm()
    {
        armInRightHand = GO_LevelManager.instance.SpawnObjects(GO_LevelManager.instance.armPlayer.gameObject, rightHandRigg.position, rightHandRigg.rotation).transform;
    }

    private void EndMiniGame()
    {
        GO_PlayerNetworkManager.localPlayer.EnddragMode(true);
        ActiveCanvas(false);
        GO_PlayerNetworkManager.localPlayer.pullMiniGame = 0;
        inputPlayer.drag = false;
    }

    public void ActiveCanvas(bool _state)
    {
        hudMinigame.gameObject.SetActive(_state);
    }
    public void ShowTimeRemain()
    {
        GO_PlayerNetworkManager.localPlayer.timeMinigame += Time.deltaTime;
        endMiniGameCount = GO_PlayerNetworkManager.localPlayer.timeMinigame >= timeMaxMiniGame;
        hudMinigame.SetRemainTime(GO_PlayerNetworkManager.localPlayer.timeMinigame / timeMaxMiniGame);

    }

    
    private void ResetMinigame()
    {
        hudMinigame.ResetPullCount();
        hudMinigame.SetRemainTime(0);
        GO_PlayerNetworkManager.localPlayer.timeMinigame = 0;
        endMiniGameCount = false;
        gameEnd = false;
        GO_PlayerNetworkManager.localPlayer.pullMiniGame = 0;
        ActiveCanvas(true);

    }

    public Transform target; // El objetivo al que quieres dirigir las manos
    public Animator animator; // El componente Animator del personaje
    private void OnAnimatorIK()
    {
        if (target != null)
        {
            //Debug.Log("IK..." + animator + " - " + target);

            animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 1);
            //animator.SetIKRotationWeight(AvatarIKGoal.RightHand, 1);
            animator.SetIKPosition(AvatarIKGoal.RightHand, target.position);
            //animator.SetIKRotation(AvatarIKGoal.RightHand, Quaternion.LookRotation(target.position - transform.position));
            
            animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1);
            //animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 1);
            animator.SetIKPosition(AvatarIKGoal.LeftHand, target.position);
            //animator.SetIKRotation(AvatarIKGoal.LeftHand, Quaternion.LookRotation(target.position - transform.position));
        }
        else
        {
            animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 0);
            //animator.SetIKRotationWeight(AvatarIKGoal.RightHand, 0);
            animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 0);
            //animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 0);
        }
    }
    private void LateUpdate()
    {
        hudMinigame.transform.LookAt(GO_MainCamera.MainCamera.transform);   
    }

    public void DropArm(bool _State)
    {
        if(rightArmPlayer.activeSelf)
        {
            rightArmPlayer.SetActive(_State);
            return;
        }
        if(leftArmPlayer.activeSelf)
        {
            leftArmPlayer.SetActive(_State);
            return;
        }
    }

    private bool ReadyForMiniGame()
    {
        return leftArmPlayer.activeSelf || rightArmPlayer.activeSelf;
    }
}