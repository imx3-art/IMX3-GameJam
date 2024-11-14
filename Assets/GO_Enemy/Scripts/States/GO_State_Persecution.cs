using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GO_State_Persecution : GO_State
{
    protected override void Awake()
    {
        base.Awake();
    }
    
    private void OnEnable()
    {
        enemy.navMeshAgent.speed = enemy.runSpeed;
        enemy.UpdateAnimation("Run");
    }

    void Update()
    {
        Transform armTransform;
        if (!enemy.hasArm && enemy.visionController.SeeTheArm(out armTransform))
        {
            // Si ve un brazo y no tiene uno, cambiar al estado de recogerlo
            GO_State_PickUpArm pickUpArmState = GetComponent<GO_State_PickUpArm>();
            pickUpArmState.SetArmTransform(armTransform);
            stateMachine.ActivateState(pickUpArmState);
            return;
        }

        Transform playerTransform;
        if (enemy.visionController.SeeThePlayer(out playerTransform))
        {
            enemy.navMeshController.followObjective = playerTransform;
            enemy.navMeshController.UpdateDestinationPoint();
        }
        else
        {
            // Si ya no ve al jugador, volver al estado de patrulla
            stateMachine.ActivateState(GetComponent<GO_State_Patrol>());
        }
    }
}