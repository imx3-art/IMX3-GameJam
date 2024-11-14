using System.Collections;
using UnityEngine;

public class GO_State_PickUpArm : GO_State
{
    [Header("Pick Up Settings")]
    public float pickUpTime = 4.0f; 

    private Transform armTransform;

    protected override void Awake()
    {
        base.Awake();
    }

    private void OnEnable()
    {
        enemy.UpdateAnimation("Walk");
    }

    private void Update()
    {
        if (armTransform == null)
        {
            stateMachine.ActivateState(GetComponent<GO_State_Patrol>());
            return;
        }

        enemy.navMeshController.UpdateDestinationPoint(armTransform.position);

        if (enemy.navMeshController.ArrivedPoint())
        {
            StartCoroutine(PickUpArmCoroutine());
        }
    }

    public void SetArmTransform(Transform arm)
    {
        armTransform = arm;
    }

    private IEnumerator PickUpArmCoroutine()
    {
        enemy.navMeshAgent.isStopped = true;
        enemy.UpdateAnimation("Idle"); 

        yield return new WaitForSeconds(pickUpTime);

        if (armTransform != null)
        {
            Destroy(armTransform.gameObject);

            armTransform = null;
        }

        enemy.hasArm = true;

        Transform playerTransform;
        if (enemy.visionController.SeeThePlayer(out playerTransform))
        {
            stateMachine.ActivateState(GetComponent<GO_State_Persecution>());
        }
        else
        {
            // Si no ve al jugador, volver al estado de patrulla
            stateMachine.ActivateState(GetComponent<GO_State_Patrol>());
        }
    }
}
