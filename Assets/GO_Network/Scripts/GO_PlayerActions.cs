using StarterAssets;
using UnityEngine;

public class GO_PlayerActions : MonoBehaviour
{
    public float maxDistance = 10f;
    public float maxUpDistance = 1.25f;
    public GO_PlayerNetworkManager otherPlayerNetworkManager;
    public LayerMask layerMask;
    GO_InputsPlayer inputPlayer;

    private void Start()
    {
        inputPlayer = GetComponent<GO_InputsPlayer>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (inputPlayer.drag)
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
            }
        }
        else if(otherPlayerNetworkManager)
        {
            target = null;
            otherPlayerNetworkManager = null;
        }
    }


    private void MiniGameDrag()
    {

    }

    public Transform target; // El objetivo al que quieres dirigir las manos
    public Animator animator; // El componente Animator del personaje
    private void OnAnimatorIK()
    {
        if (target != null)
        {
            Debug.Log("IK..." + animator + " - " + target);

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
}